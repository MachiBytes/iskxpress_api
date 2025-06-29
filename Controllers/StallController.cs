using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Services;
using iskxpress_api.Models;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api/stalls")]
public class StallController : ControllerBase
{
    private readonly IStallService _stallService;

    public StallController(IStallService stallService)
    {
        _stallService = stallService;
    }

    /// <summary>
    /// Get all stalls
    /// </summary>
    /// <returns>List of all stalls</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StallResponse>>> GetAllStalls()
    {
        var stalls = await _stallService.GetAllStallsAsync();
        return Ok(stalls);
    }

    /// <summary>
    /// Get a specific stall by ID
    /// </summary>
    /// <param name="stallId">The ID of the stall</param>
    /// <returns>The stall information</returns>
    [HttpGet("{stallId}")]
    public async Task<ActionResult<StallResponse>> GetStall(int stallId)
    {
        var stall = await _stallService.GetStallByIdAsync(stallId);
        if (stall == null)
        {
            return NotFound($"Stall with ID {stallId} not found");
        }

        return Ok(stall);
    }

    /// <summary>
    /// Get the stall for a vendor
    /// </summary>
    /// <param name="vendorId">The ID of the vendor</param>
    /// <returns>The stall owned by the vendor</returns>
    [HttpGet("vendor/{vendorId}")]
    public async Task<ActionResult<StallResponse>> GetStallByVendor(int vendorId)
    {
        var stall = await _stallService.GetStallByVendorIdAsync(vendorId);
        if (stall == null)
        {
            return NotFound($"No stall found for vendor {vendorId}");
        }
        return Ok(stall);
    }

    /// <summary>
    /// Create a new stall
    /// </summary>
    /// <param name="request">The stall information including vendor ID</param>
    /// <returns>The created stall information</returns>
    [HttpPost]
    public async Task<ActionResult<StallResponse>> CreateStall([FromBody] CreateStallRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdStall = await _stallService.CreateStallAsync(request);
        if (createdStall == null)
        {
            return Conflict($"Vendor {request.VendorId} already has a stall");
        }

        return CreatedAtAction(nameof(GetStall), new { stallId = createdStall.Id }, createdStall);
    }

    /// <summary>
    /// Update stall information
    /// </summary>
    /// <param name="stallId">The ID of the stall to update</param>
    /// <param name="request">The updated stall information</param>
    /// <returns>The updated stall information</returns>
    [HttpPut("{stallId}")]
    public async Task<ActionResult<StallResponse>> UpdateStall(
        int stallId, 
        [FromBody] UpdateStallRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedStall = await _stallService.UpdateStallAsync(stallId, request);
        if (updatedStall == null)
        {
            return NotFound($"Stall with ID {stallId} not found");
        }

        return Ok(updatedStall);
    }
} 