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

    public OrderService(
        IskExpressDbContext context,
        IOrderRepository orderRepository,
        ICartItemRepository cartItemRepository,
        IProductRepository productRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
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

        // Create order items and calculate total
        decimal totalPrice = 0;
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in stallCartItems)
        {
            var product = products.First(p => p.Id == cartItem.ProductId);
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                PriceEach = product.PriceWithMarkup
            };

            orderItems.Add(orderItem);
            totalPrice += product.PriceWithMarkup * cartItem.Quantity;
        }

        order.TotalPrice = totalPrice;
        order.OrderItems = orderItems;

        // Save order and remove cart items in a transaction
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

                decimal totalPrice = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in stallCartItems)
                {
                    var product = products.First(p => p.Id == cartItem.ProductId);
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceEach = product.PriceWithMarkup
                    };
                    orderItems.Add(orderItem);
                    totalPrice += product.PriceWithMarkup * cartItem.Quantity;
                }

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