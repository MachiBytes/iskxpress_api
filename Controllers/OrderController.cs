using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Orders;
using iskxpress_api.Services;
using iskxpress_api.Models;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order from cart items (checkout)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The checkout request</param>
    /// <returns>The created order</returns>
    [HttpPost("user/{userId}/checkout")]
    public async Task<ActionResult<OrderResponse>> Checkout(int userId, [FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(userId, request);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Multi-stall checkout: Creates multiple orders from cart items belonging to different stalls
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The checkout request</param>
    /// <returns>The created orders</returns>
    [HttpPost("user/{userId}/multi-checkout")]
    public async Task<ActionResult<MultiOrderResponse>> MultiCheckout(int userId, [FromBody] CreateOrderRequest request)
    {
        try
        {
            var result = await _orderService.CreateMultiOrderAsync(userId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all orders for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="status">Optional order status filter</param>
    /// <returns>Collection of user orders</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetUserOrders(int userId, [FromQuery] OrderStatus? status = null)
    {
        try
        {
            var orders = await _orderService.GetUserOrdersAsync(userId, status);
            return Ok(orders);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all orders for a stall
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <param name="status">Optional order status filter</param>
    /// <returns>Collection of stall orders</returns>
    [HttpGet("stall/{stallId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetStallOrders(int stallId, [FromQuery] OrderStatus? status = null)
    {
        try
        {
            var orders = await _orderService.GetStallOrdersAsync(stallId, status);
            return Ok(orders);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all orders in the system
    /// </summary>
    /// <param name="hasDeliveryPartner">Optional filter for orders with/without delivery partner</param>
    /// <returns>Collection of all orders</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders([FromQuery] bool? hasDeliveryPartner = null)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync(hasDeliveryPartner);
            return Ok(orders);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all orders for a stall that have a delivery partner assigned
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <returns>Collection of stall orders with delivery partner</returns>
    [HttpGet("stall/{stallId}/with-delivery-partner")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetStallOrdersWithDeliveryPartner(int stallId)
    {
        try
        {
            var orders = await _orderService.GetStallOrdersWithDeliveryPartnerAsync(stallId);
            return Ok(orders);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all orders for a delivery partner
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <param name="isFinished">Optional filter for finished (accomplished/rejected) or ongoing orders</param>
    /// <returns>Collection of delivery partner orders</returns>
    [HttpGet("delivery-partner/{deliveryPartnerId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetDeliveryPartnerOrders(int deliveryPartnerId, [FromQuery] bool? isFinished = null)
    {
        try
        {
            var orders = await _orderService.GetDeliveryPartnerOrdersAsync(deliveryPartnerId, isFinished);
            return Ok(orders);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets a specific order by ID
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>The order details</returns>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(int orderId)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(orderId);
            
            if (order == null)
            {
                return NotFound("Order not found");
            }
            
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates the status of an order
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="request">The status update request</param>
    /// <returns>The updated order</returns>
    [HttpPut("{orderId}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Assigns a delivery partner to an order
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="request">The delivery partner assignment request</param>
    /// <returns>The updated order</returns>
    [HttpPut("{orderId}/assign-delivery-partner")]
    public async Task<ActionResult<OrderResponse>> AssignDeliveryPartner(int orderId, [FromBody] AssignDeliveryPartnerRequest request)
    {
        try
        {
            var order = await _orderService.AssignDeliveryPartnerAsync(orderId, request.DeliveryPartnerId);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Rejects an order with a reason
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="request">The order rejection request</param>
    /// <returns>The updated order</returns>
    [HttpPut("{orderId}/reject")]
    public async Task<ActionResult<OrderResponse>> RejectOrder(int orderId, [FromBody] RejectOrderRequest request)
    {
        try
        {
            var order = await _orderService.RejectOrderAsync(orderId, request.RejectionReason);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 