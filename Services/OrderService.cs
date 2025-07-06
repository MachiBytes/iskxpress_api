using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.DTOs.Orders;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Services;

public class OrderService : IOrderService
{
    private readonly IskExpressDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStallRepository _stallRepository;

    public OrderService(
        IskExpressDbContext context,
        IOrderRepository orderRepository,
        ICartItemRepository cartItemRepository,
        IProductRepository productRepository,
        IStallRepository stallRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _stallRepository = stallRepository;
    }

    public async Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        // Validate request
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            throw new ArgumentException("Delivery address is required when fulfillment method is Delivery");
        }

        // Load user
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        // Get cart items and validate they belong to the user
        var cartItems = await _cartItemRepository.GetByIdsAsync(request.CartItemIds);
        var userCartItems = cartItems.Where(ci => ci.UserId == userId).ToList();

        if (userCartItems.Count != request.CartItemIds.Count)
        {
            throw new ArgumentException("Some cart items do not belong to the user or do not exist");
        }

        if (!userCartItems.Any())
        {
            throw new ArgumentException("No valid cart items found");
        }

        // Group cart items by stall (since we create separate orders per stall)
        var cartItemsByStall = userCartItems.GroupBy(ci => ci.StallId).ToList();

        // For now, we'll only allow checkout from one stall at a time
        if (cartItemsByStall.Count > 1)
        {
            throw new ArgumentException("Cannot checkout items from multiple stalls in a single order");
        }

        var stallCartItems = cartItemsByStall.First();
        var stallId = stallCartItems.Key;

        // Validate products still exist and are available
        var productIds = stallCartItems.Select(ci => ci.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);

        if (products.Count() != productIds.Count)
        {
            throw new ArgumentException("Some products no longer exist");
        }

        // Check product availability
        var unavailableProducts = products.Where(p => p.Availability != ProductAvailability.Available).ToList();
        if (unavailableProducts.Any())
        {
            var productNames = string.Join(", ", unavailableProducts.Select(p => p.Name));
            throw new ArgumentException($"The following products are no longer available: {productNames}");
        }

        // Get stall information to check delivery availability
        var stall = await _stallRepository.GetByIdAsync(stallId);
        if (stall == null)
        {
            throw new ArgumentException($"Stall with ID {stallId} not found");
        }

        // Create order
        var order = new Order
        {
            UserId = userId,
            StallId = stallId,
            Status = OrderStatus.Pending,
            FulfillmentMethod = request.FulfillmentMethod,
            DeliveryAddress = request.DeliveryAddress,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Handle delivery partner assignment
        if (request.FulfillmentMethod == FulfillmentMethod.Pickup)
        {
            // For pickup orders, assign the stall vendor as the delivery partner
            // since they're responsible for handling the pickup
            order.DeliveryPartnerId = stall.VendorId;
        }
        else if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
        {
            if (stall.DeliveryAvailable)
            {
                // For delivery orders, if stall has delivery available, assign the vendor as delivery partner
                order.DeliveryPartnerId = stall.VendorId;
            }
            else
            {
                // Stall doesn't have delivery available - leave delivery partner unassigned
                // Order will need manual delivery partner assignment later
            }
        }

        // Create order items and calculate totals
        decimal totalPrice = 0;
        decimal totalCommissionFee = 0;
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in stallCartItems)
        {
            var product = products.First(p => p.Id == cartItem.ProductId);
            
            // Use PremiumUserPrice for premium users, otherwise PriceWithMarkup
            decimal pricePerItem = user.Premium ? product.PremiumUserPrice : product.PriceWithMarkup;
            decimal commissionPerItem = pricePerItem - product.BasePrice;
            
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                PriceEach = pricePerItem,
                CommissionFee = commissionPerItem * cartItem.Quantity
            };

            orderItems.Add(orderItem);
            totalPrice += pricePerItem * cartItem.Quantity;
            totalCommissionFee += commissionPerItem * cartItem.Quantity;
        }

        // Calculate delivery fee
        decimal deliveryFee = 0.00m;
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
        {
            deliveryFee = user.Premium ? 0.00m : 10.00m;
        }

        order.TotalPrice = totalPrice;
        order.TotalCommissionFee = totalCommissionFee;
        order.DeliveryFee = deliveryFee;
        order.OrderItems = orderItems;

        // Save order and remove cart items in a transaction
        var providerName = _context.Database.ProviderName;
        if (providerName != "Microsoft.EntityFrameworkCore.InMemory")
        {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _orderRepository.AddAsync(order);
            await _context.SaveChangesAsync();

            // Remove cart items
            foreach (var cartItem in stallCartItems)
            {
                await _cartItemRepository.DeleteAsync(cartItem.Id);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Return the created order
            var createdOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id);
            return MapToOrderResponse(createdOrder!);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
        else
        {
            // InMemory provider does not support transactions
            await _orderRepository.AddAsync(order);
            await _context.SaveChangesAsync();

            // Remove cart items
            foreach (var cartItem in stallCartItems)
            {
                await _cartItemRepository.DeleteAsync(cartItem.Id);
            }

            await _context.SaveChangesAsync();

            // Return the created order
            var createdOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id);
            return MapToOrderResponse(createdOrder!);
        }
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId, OrderStatus? status = null)
    {
        var orders = await _orderRepository.GetUserOrdersAsync(userId, status);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetStallOrdersAsync(int stallId, OrderStatus? status = null)
    {
        var orders = await _orderRepository.GetStallOrdersAsync(stallId, status);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(bool? hasDeliveryPartner = null)
    {
        var orders = await _orderRepository.GetAllOrdersAsync(hasDeliveryPartner);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetStallOrdersWithDeliveryPartnerAsync(int stallId)
    {
        var orders = await _orderRepository.GetStallOrdersWithDeliveryPartnerAsync(stallId);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetDeliveryPartnerOrdersAsync(int deliveryPartnerId, bool? isFinished = null)
    {
        var orders = await _orderRepository.GetDeliveryPartnerOrdersAsync(deliveryPartnerId, isFinished);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<OrderResponse?> GetOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        return order != null ? MapToOrderResponse(order) : null;
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {orderId} not found");
        }

        // Validate status transition
        if (!IsValidStatusTransition(order.Status, newStatus, order.FulfillmentMethod))
        {
            throw new ArgumentException($"Invalid status transition from {order.Status} to {newStatus}");
        }

        // Check if order is being completed (status changing to Accomplished)
        bool isOrderBeingCompleted = order.Status != OrderStatus.Accomplished && newStatus == OrderStatus.Accomplished;

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        // If order is being completed, add commission fees to stall's pending fees
        if (isOrderBeingCompleted)
        {
            var stall = await _stallRepository.GetByIdAsync(order.StallId);
            if (stall != null)
            {
                stall.PendingFees += order.TotalCommissionFee;
                await _stallRepository.UpdateAsync(stall);
            }
        }

        // Return the updated order
        var updatedOrder = await _orderRepository.GetOrderWithItemsAsync(orderId);
        return MapToOrderResponse(updatedOrder!);
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus, FulfillmentMethod fulfillmentMethod)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.Preparing || newStatus == OrderStatus.Rejected,
            OrderStatus.Preparing => newStatus == OrderStatus.ToDeliver || newStatus == OrderStatus.ToReceive || newStatus == OrderStatus.Rejected,
            OrderStatus.ToDeliver => newStatus == OrderStatus.ToReceive,
            OrderStatus.ToReceive => newStatus == OrderStatus.Accomplished,
            OrderStatus.Accomplished => false, // Cannot change from accomplished
            OrderStatus.Rejected => false, // Cannot change from rejected
            _ => false
        };
    }

    public async Task<MultiOrderResponse> CreateMultiOrderAsync(int userId, CreateOrderRequest request)
    {
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            throw new ArgumentException("Delivery address is required when fulfillment method is Delivery");
        }

        // Load user
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        var cartItems = await _cartItemRepository.GetByIdsAsync(request.CartItemIds);
        var userCartItems = cartItems.Where(ci => ci.UserId == userId).ToList();

        if (userCartItems.Count != request.CartItemIds.Count)
        {
            throw new ArgumentException("Some cart items do not belong to the user or do not exist");
        }

        if (!userCartItems.Any())
        {
            throw new ArgumentException("No valid cart items found");
        }

        var cartItemsByStall = userCartItems.GroupBy(ci => ci.StallId).ToList();
        var createdOrders = new List<OrderResponse>();

        // Save orders and remove cart items in a transaction
        var providerName = _context.Database.ProviderName;
        if (providerName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var stallGroup in cartItemsByStall)
                {
                    var stallId = stallGroup.Key;
                    var stallCartItems = stallGroup.ToList();
                    var productIds = stallCartItems.Select(ci => ci.ProductId).ToList();
                    var products = await _productRepository.GetByIdsAsync(productIds);

                    if (products.Count() != productIds.Count)
                    {
                        throw new ArgumentException("Some products no longer exist");
                    }

                    var unavailableProducts = products.Where(p => p.Availability != ProductAvailability.Available).ToList();
                    if (unavailableProducts.Any())
                    {
                        var productNames = string.Join(", ", unavailableProducts.Select(p => p.Name));
                        throw new ArgumentException($"The following products are no longer available: {productNames}");
                    }

                    // Get stall information to assign delivery partner for pickup orders
                    var stall = await _stallRepository.GetByIdAsync(stallId);
                    if (stall == null)
                    {
                        throw new ArgumentException($"Stall with ID {stallId} not found");
                    }

                    var order = new Order
                    {
                        UserId = userId,
                        StallId = stallId,
                        Status = OrderStatus.Pending,
                        FulfillmentMethod = request.FulfillmentMethod,
                        DeliveryAddress = request.DeliveryAddress,
                        Notes = request.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Handle delivery partner assignment
                    if (request.FulfillmentMethod == FulfillmentMethod.Pickup)
                    {
                        // For pickup orders, assign the stall vendor as the delivery partner
                        // since they're responsible for handling the pickup
                        order.DeliveryPartnerId = stall.VendorId;
                    }
                    else if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
                    {
                        if (stall.DeliveryAvailable)
                        {
                            // For delivery orders, if stall has delivery available, assign the vendor as delivery partner
                            order.DeliveryPartnerId = stall.VendorId;
                        }
                        else
                        {
                            // Stall doesn't have delivery available - leave delivery partner unassigned
                            // Order will need manual delivery partner assignment later
                        }
                    }

                    decimal totalPrice = 0;
                    decimal totalCommissionFee = 0;
                    var orderItems = new List<OrderItem>();

                    foreach (var cartItem in stallCartItems)
                    {
                        var product = products.First(p => p.Id == cartItem.ProductId);
                        
                        // Use PremiumUserPrice for premium users, otherwise PriceWithMarkup
                        decimal pricePerItem = user.Premium ? product.PremiumUserPrice : product.PriceWithMarkup;
                        decimal commissionPerItem = pricePerItem - product.BasePrice;
                        
                        var orderItem = new OrderItem
                        {
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            PriceEach = pricePerItem,
                            CommissionFee = commissionPerItem * cartItem.Quantity
                        };
                        orderItems.Add(orderItem);
                        totalPrice += pricePerItem * cartItem.Quantity;
                        totalCommissionFee += commissionPerItem * cartItem.Quantity;
                    }

                    // Calculate delivery fee
                    decimal deliveryFee = 0.00m;
                    if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
                    {
                        deliveryFee = user.Premium ? 0.00m : 10.00m;
                    }

                    order.TotalPrice = totalPrice;
                    order.TotalCommissionFee = totalCommissionFee;
                    order.DeliveryFee = deliveryFee;
                    order.OrderItems = orderItems;

                    await _orderRepository.AddAsync(order);
                    await _context.SaveChangesAsync();

                    foreach (var cartItem in stallCartItems)
                    {
                        await _cartItemRepository.DeleteAsync(cartItem.Id);
                    }
                    await _context.SaveChangesAsync();

                    var createdOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id);
                    createdOrders.Add(MapToOrderResponse(createdOrder!));
                }

                await transaction.CommitAsync();
                return new MultiOrderResponse { Orders = createdOrders };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        else
        {
            // InMemory provider does not support transactions
            foreach (var stallGroup in cartItemsByStall)
            {
                var stallId = stallGroup.Key;
                var stallCartItems = stallGroup.ToList();
                var productIds = stallCartItems.Select(ci => ci.ProductId).ToList();
                var products = await _productRepository.GetByIdsAsync(productIds);

                if (products.Count() != productIds.Count)
                {
                    throw new ArgumentException("Some products no longer exist");
                }

                var unavailableProducts = products.Where(p => p.Availability != ProductAvailability.Available).ToList();
                if (unavailableProducts.Any())
                {
                    var productNames = string.Join(", ", unavailableProducts.Select(p => p.Name));
                    throw new ArgumentException($"The following products are no longer available: {productNames}");
                }

                // Get stall information to assign delivery partner for pickup orders
                var stall = await _stallRepository.GetByIdAsync(stallId);
                if (stall == null)
                {
                    throw new ArgumentException($"Stall with ID {stallId} not found");
                }

                var order = new Order
                {
                    UserId = userId,
                    StallId = stallId,
                    Status = OrderStatus.Pending,
                    FulfillmentMethod = request.FulfillmentMethod,
                    DeliveryAddress = request.DeliveryAddress,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle delivery partner assignment
                if (request.FulfillmentMethod == FulfillmentMethod.Pickup)
                {
                    // For pickup orders, assign the stall vendor as the delivery partner
                    // since they're responsible for handling the pickup
                    order.DeliveryPartnerId = stall.VendorId;
                }
                else if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
                {
                    if (stall.DeliveryAvailable)
                    {
                        // For delivery orders, if stall has delivery available, assign the vendor as delivery partner
                        order.DeliveryPartnerId = stall.VendorId;
                    }
                    else
                    {
                        // Stall doesn't have delivery available - leave delivery partner unassigned
                        // Order will need manual delivery partner assignment later
                    }
                }

                decimal totalPrice = 0;
                decimal totalCommissionFee = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in stallCartItems)
                {
                    var product = products.First(p => p.Id == cartItem.ProductId);
                    
                    // Use PremiumUserPrice for premium users, otherwise PriceWithMarkup
                    decimal pricePerItem = user.Premium ? product.PremiumUserPrice : product.PriceWithMarkup;
                    decimal commissionPerItem = pricePerItem - product.BasePrice;
                    
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceEach = pricePerItem,
                        CommissionFee = commissionPerItem * cartItem.Quantity
                    };
                    orderItems.Add(orderItem);
                    totalPrice += pricePerItem * cartItem.Quantity;
                    totalCommissionFee += commissionPerItem * cartItem.Quantity;
                }

                // Calculate delivery fee
                decimal deliveryFee = 0.00m;
                if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
                {
                    deliveryFee = user.Premium ? 0.00m : 10.00m;
                }

                order.TotalPrice = totalPrice;
                order.TotalCommissionFee = totalCommissionFee;
                order.DeliveryFee = deliveryFee;
                order.OrderItems = orderItems;

                await _orderRepository.AddAsync(order);
                await _context.SaveChangesAsync();

                foreach (var cartItem in stallCartItems)
                {
                    await _cartItemRepository.DeleteAsync(cartItem.Id);
                }
                await _context.SaveChangesAsync();

                var createdOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id);
                createdOrders.Add(MapToOrderResponse(createdOrder!));
            }

            return new MultiOrderResponse { Orders = createdOrders };
        }
    }

    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            StallId = order.StallId,
            StallName = order.Stall.Name,
            Status = order.Status,
            FulfillmentMethod = order.FulfillmentMethod,
            DeliveryAddress = order.DeliveryAddress,
            Notes = order.Notes,
            DeliveryPartnerId = order.DeliveryPartnerId,
            TotalPrice = order.TotalPrice,
            TotalCommissionFee = order.TotalCommissionFee,
            DeliveryFee = order.DeliveryFee,
            RejectionReason = order.RejectionReason,
            CreatedAt = order.CreatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                ProductDescription = string.Empty, // Product model doesn't have Description
                ProductPictureUrl = oi.Product.Picture?.ObjectUrl ?? string.Empty,
                Quantity = oi.Quantity,
                PriceEach = oi.PriceEach,
                CommissionFee = oi.CommissionFee,
                TotalPrice = oi.PriceEach * oi.Quantity
            }).ToList()
        };
    }

    public async Task<OrderResponse> AssignDeliveryPartnerAsync(int orderId, int deliveryPartnerId)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {orderId} not found");
        }

        if (order.FulfillmentMethod == FulfillmentMethod.Pickup)
        {
            throw new ArgumentException("Cannot manually assign delivery partner to pickup orders - vendor is automatically assigned");
        }

        if (order.FulfillmentMethod != FulfillmentMethod.Delivery)
        {
            throw new ArgumentException("Can only assign delivery partner to delivery orders");
        }

        if (order.DeliveryPartnerId.HasValue)
        {
            throw new ArgumentException("Order already has a delivery partner assigned");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new ArgumentException("Can only assign delivery partner to pending orders");
        }

        // Check if delivery partner already has maximum ongoing orders (3)
        var ongoingOrders = await _orderRepository.GetDeliveryPartnerOrdersAsync(deliveryPartnerId, isFinished: false);
        var ongoingOrderCount = ongoingOrders.Count();
        
        if (ongoingOrderCount >= 3)
        {
            throw new ArgumentException($"Delivery partner already has {ongoingOrderCount} ongoing orders. Maximum allowed is 3.");
        }

        order.DeliveryPartnerId = deliveryPartnerId;
        await _context.SaveChangesAsync();

        // Return the updated order
        var updatedOrder = await _orderRepository.GetOrderWithItemsAsync(orderId);
        return MapToOrderResponse(updatedOrder!);
    }

    public async Task<OrderResponse> RejectOrderAsync(int orderId, string rejectionReason)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {orderId} not found");
        }

        if (order.Status == OrderStatus.Accomplished || order.Status == OrderStatus.Rejected)
        {
            throw new ArgumentException($"Cannot reject order with status {order.Status}");
        }

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new ArgumentException("Rejection reason is required");
        }

        order.Status = OrderStatus.Rejected;
        order.RejectionReason = rejectionReason;
        await _context.SaveChangesAsync();

        // Return the updated order
        var updatedOrder = await _orderRepository.GetOrderWithItemsAsync(orderId);
        return MapToOrderResponse(updatedOrder!);
    }
} 