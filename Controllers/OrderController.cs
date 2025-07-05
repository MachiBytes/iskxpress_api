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
} 