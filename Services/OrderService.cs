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
    private readonly IDeliveryRequestRepository _deliveryRequestRepository;
    private readonly IOrderConfirmationRepository _orderConfirmationRepository;

    public OrderService(
        IskExpressDbContext context,
        IOrderRepository orderRepository,
        ICartItemRepository cartItemRepository,
        IProductRepository productRepository,
        IStallRepository stallRepository,
        IDeliveryRequestRepository deliveryRequestRepository,
        IOrderConfirmationRepository orderConfirmationRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _stallRepository = stallRepository;
        _deliveryRequestRepository = deliveryRequestRepository;
        _orderConfirmationRepository = orderConfirmationRepository;
    }

    public async Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        // Validate request
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            throw new ArgumentException("Delivery address is required when fulfillment method is Delivery");
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

        // Handle delivery partner assignment based on stall availability
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery)
        {
            if (stall.hasDelivery && stall.DeliveryAvailable)
            {
                // Stall has delivery partner available - assign immediately
                // Note: In a real implementation, you might want to assign to a specific partner
                // For now, we'll leave it unassigned and let the delivery system handle it
            }
            else
            {
                // Stall doesn't have delivery partner - order will be created but won't show in stall orders
                // until a delivery partner is assigned
            }
        }

        // Create order items and calculate total
        decimal totalSellingPrice = 0;
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in stallCartItems)
        {
            var product = products.First(p => p.Id == cartItem.ProductId);
            
            // Always use PriceWithMarkup for products
            decimal pricePerItem = product.PriceWithMarkup;
            
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                PriceEach = pricePerItem
            };

            orderItems.Add(orderItem);
            totalSellingPrice += pricePerItem * cartItem.Quantity;
        }

        // Calculate delivery fee
        decimal deliveryFee = request.FulfillmentMethod == FulfillmentMethod.Delivery ? 10.00m : 0.00m;
        
        // Calculate total price
        decimal totalPrice = totalSellingPrice + deliveryFee;

        order.TotalPrice = totalPrice;
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

        // Handle special cases for status changes
        if (newStatus == OrderStatus.ToReceive)
        {
            // Create order confirmation for user
            await CreateOrderConfirmationAsync(orderId);
        }

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        // Return the updated order
        var updatedOrder = await _orderRepository.GetOrderWithItemsAsync(orderId);
        return MapToOrderResponse(updatedOrder!);
    }

    public async Task<OrderConfirmationResponse> ConfirmOrderDeliveryAsync(int orderId)
    {
        var confirmation = await _orderConfirmationRepository.GetByOrderIdAsync(orderId);
        if (confirmation == null)
        {
            throw new ArgumentException($"No confirmation request found for order {orderId}");
        }

        if (confirmation.IsConfirmed || confirmation.IsAutoConfirmed)
        {
            throw new ArgumentException("Order has already been confirmed");
        }

        confirmation.IsConfirmed = true;
        confirmation.ConfirmedAt = DateTime.UtcNow;

        // Update order status to accomplished
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Accomplished;
        }

        await _context.SaveChangesAsync();

        return new OrderConfirmationResponse
        {
            Id = confirmation.Id,
            OrderId = confirmation.OrderId,
            CreatedAt = confirmation.CreatedAt,
            ConfirmationDeadline = confirmation.ConfirmationDeadline,
            IsConfirmed = confirmation.IsConfirmed,
            ConfirmedAt = confirmation.ConfirmedAt,
            IsAutoConfirmed = confirmation.IsAutoConfirmed,
            AutoConfirmedAt = confirmation.AutoConfirmedAt
        };
    }

    public async Task ProcessAutoConfirmationsAsync()
    {
        var pendingConfirmations = await _orderConfirmationRepository.GetPendingAutoConfirmationsAsync();
        
        foreach (var confirmation in pendingConfirmations)
        {
            confirmation.IsAutoConfirmed = true;
            confirmation.AutoConfirmedAt = DateTime.UtcNow;

            // Update order status to accomplished
            var order = await _orderRepository.GetOrderWithItemsAsync(confirmation.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Accomplished;
            }
        }

        if (pendingConfirmations.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private async Task CreateOrderConfirmationAsync(int orderId)
    {
        // Check if confirmation already exists
        var existingConfirmation = await _orderConfirmationRepository.GetByOrderIdAsync(orderId);
        if (existingConfirmation != null)
        {
            return; // Confirmation already exists
        }

        var confirmation = new OrderConfirmation
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            ConfirmationDeadline = DateTime.UtcNow.AddMinutes(5), // 5 minutes deadline
            IsConfirmed = false,
            IsAutoConfirmed = false
        };

        await _orderConfirmationRepository.AddAsync(confirmation);
        await _context.SaveChangesAsync();
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus, FulfillmentMethod fulfillmentMethod)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.ToPrepare,
            OrderStatus.ToPrepare => newStatus == OrderStatus.ToDeliver || newStatus == OrderStatus.ToReceive,
            OrderStatus.ToDeliver => newStatus == OrderStatus.ToReceive,
            OrderStatus.ToReceive => newStatus == OrderStatus.Accomplished,
            OrderStatus.Accomplished => false, // Cannot change from accomplished
            _ => false
        };
    }

    public async Task<MultiOrderResponse> CreateMultiOrderAsync(int userId, CreateOrderRequest request)
    {
        if (request.FulfillmentMethod == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            throw new ArgumentException("Delivery address is required when fulfillment method is Delivery");
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

                decimal totalSellingPrice = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in stallCartItems)
                {
                    var product = products.First(p => p.Id == cartItem.ProductId);
                    
                    // Always use PriceWithMarkup for products
                    decimal pricePerItem = product.PriceWithMarkup;
                    
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceEach = pricePerItem
                    };
                    orderItems.Add(orderItem);
                    totalSellingPrice += pricePerItem * cartItem.Quantity;
                }

                // Calculate delivery fee
                decimal deliveryFee = request.FulfillmentMethod == FulfillmentMethod.Delivery ? 10.00m : 0.00m;
                
                // Calculate total price
                decimal totalPrice = totalSellingPrice + deliveryFee;

                order.TotalPrice = totalPrice;
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

    private static OrderResponse MapToOrderResponse(Order order)
    {
        // Calculate pricing breakdown
        decimal totalSellingPrice = order.OrderItems.Sum(oi => oi.PriceEach * oi.Quantity);
        decimal deliveryFee = order.FulfillmentMethod == FulfillmentMethod.Delivery ? 10.00m : 0.00m;
        
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
            TotalSellingPrice = totalSellingPrice,
            DeliveryFee = deliveryFee,
            TotalPrice = order.TotalPrice,
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
                TotalPrice = oi.PriceEach * oi.Quantity
            }).ToList()
        };
    }
} 