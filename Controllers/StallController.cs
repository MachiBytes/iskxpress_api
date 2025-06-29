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
    private readonly ILogger<StallController> _logger;

    public StallController(IStallService stallService, ILogger<StallController> logger)
    {
        _stallService = stallService;
        _logger = logger;
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
    /// Create a new stall for a vendor
    /// </summary>
    /// <param name="vendorId">The ID of the vendor who will own the stall</param>
    /// <param name="request">The stall information</param>
    /// <returns>The created stall information</returns>
    [HttpPost("vendor/{vendorId}")]
    public async Task<ActionResult<StallResponse>> CreateStall(int vendorId, [FromBody] CreateStallRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdStall = await _stallService.CreateStallAsync(vendorId, request);
        if (createdStall == null)
        {
            return Conflict($"Vendor {vendorId} already has a stall");
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

    /// <summary>
    /// Upload or replace stall display picture
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <param name="file">The image file to upload</param>
    /// <returns>Updated stall with new display picture</returns>
    /// <response code="200">Stall picture uploaded successfully</response>
    /// <response code="400">Invalid file or request</response>
    /// <response code="404">Stall not found</response>
    /// <response code="413">File too large</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{stallId}/upload-picture")]
    [ProducesResponseType(typeof(StallResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(413)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<StallResponse>> UploadStallPicture(int stallId, IFormFile file)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            // Check file size (5MB limit)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return StatusCode(413, "File size exceeds 5MB limit");

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType?.ToLowerInvariant()))
                return BadRequest("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed");

            // Check if stall exists
            var existingStall = await _stallService.GetStallByIdAsync(stallId);
            if (existingStall == null)
                return NotFound($"Stall with ID {stallId} not found");

            // Upload the file (this will automatically replace any existing stall picture)
            var updatedStall = await _stallService.UploadStallPictureAsync(stallId, file);
            if (updatedStall == null)
                return NotFound($"Stall with ID {stallId} not found");

            return Ok(updatedStall);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading stall picture for stall {StallId}", stallId);
            return StatusCode(500, "Internal server error");
        }
    }
} 