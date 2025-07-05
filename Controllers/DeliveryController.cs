using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Delivery;
using iskxpress_api.Services;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// Gets all available delivery requests (pending assignment)
    /// </summary>
    /// <returns>Collection of pending delivery requests</returns>
    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<DeliveryRequestResponse>>> GetAvailableDeliveryRequests()
    {
        try
        {
            var requests = await _deliveryService.GetAvailableDeliveryRequestsAsync();
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets delivery requests assigned to a specific delivery partner
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>Collection of delivery requests assigned to the partner</returns>
    [HttpGet("partner/{deliveryPartnerId}/requests")]
    public async Task<ActionResult<IEnumerable<DeliveryRequestResponse>>> GetDeliveryPartnerRequests(int deliveryPartnerId)
    {
        try
        {
            var requests = await _deliveryService.GetDeliveryPartnerRequestsAsync(deliveryPartnerId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets a specific delivery request by ID
    /// </summary>
    /// <param name="requestId">The delivery request ID</param>
    /// <returns>The delivery request details</returns>
    [HttpGet("requests/{requestId}")]
    public async Task<ActionResult<DeliveryRequestResponse>> GetDeliveryRequest(int requestId)
    {
        try
        {
            var request = await _deliveryService.GetDeliveryRequestAsync(requestId);
            
            if (request == null)
            {
                return NotFound("Delivery request not found");
            }
            
            return Ok(request);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Assigns a delivery request to a delivery partner
    /// </summary>
    /// <param name="requestId">The delivery request ID</param>
    /// <param name="assignRequest">The assignment request</param>
    /// <returns>The updated delivery request</returns>
    [HttpPost("requests/{requestId}/assign")]
    public async Task<ActionResult<DeliveryRequestResponse>> AssignDeliveryRequest(int requestId, [FromBody] AssignDeliveryRequestRequest assignRequest)
    {
        try
        {
            var request = await _deliveryService.AssignDeliveryRequestAsync(requestId, assignRequest.DeliveryPartnerId);
            return Ok(request);
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
    /// Updates the status of a delivery request
    /// </summary>
    /// <param name="requestId">The delivery request ID</param>
    /// <param name="status">The new status</param>
    /// <returns>The updated delivery request</returns>
    [HttpPut("requests/{requestId}/status")]
    public async Task<ActionResult<DeliveryRequestResponse>> UpdateDeliveryRequestStatus(int requestId, [FromQuery] int status)
    {
        try
        {
            var deliveryRequestStatus = (iskxpress_api.Models.DeliveryRequestStatus)status;
            var request = await _deliveryService.UpdateDeliveryRequestStatusAsync(requestId, deliveryRequestStatus);
            return Ok(request);
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