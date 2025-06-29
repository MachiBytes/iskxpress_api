using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Services;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api")]
public class SectionController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    /// <summary>
    /// Get all sections for a specific stall
    /// </summary>
    /// <param name="stallId">The ID of the stall</param>
    /// <returns>List of sections in the stall</returns>
    [HttpGet("stalls/{stallId}/sections")]
    public async Task<ActionResult<IEnumerable<SectionResponse>>> GetSectionsByStall(int stallId)
    {
        var sections = await _sectionService.GetSectionsByStallIdAsync(stallId);
        return Ok(sections);
    }

    /// <summary>
    /// Create a new section for a stall
    /// </summary>
    /// <param name="stallId">The ID of the stall</param>
    /// <param name="request">The section information</param>
    /// <returns>The created section information</returns>
    [HttpPost("stalls/{stallId}/sections")]
    public async Task<ActionResult<SectionResponse>> CreateSection(
        int stallId,
        [FromBody] CreateSectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdSection = await _sectionService.CreateSectionAsync(stallId, request);
        if (createdSection == null)
        {
            return BadRequest($"Unable to create section. Stall {stallId} may not exist.");
        }

        return CreatedAtAction(nameof(GetSection), new { sectionId = createdSection.Id }, createdSection);
    }

    /// <summary>
    /// Get a section by ID
    /// </summary>
    /// <param name="sectionId">The ID of the section</param>
    /// <returns>The section information</returns>
    [HttpGet("sections/{sectionId}")]
    public async Task<ActionResult<SectionResponse>> GetSection(int sectionId)
    {
        var section = await _sectionService.GetSectionByIdAsync(sectionId);
        if (section == null)
        {
            return NotFound($"Section with ID {sectionId} not found");
        }

        return Ok(section);
    }

    /// <summary>
    /// Update a section
    /// </summary>
    /// <param name="sectionId">The ID of the section to update</param>
    /// <param name="request">The updated section information</param>
    /// <returns>The updated section information</returns>
    [HttpPut("sections/{sectionId}")]
    public async Task<ActionResult<SectionResponse>> UpdateSection(
        int sectionId,
        [FromBody] UpdateSectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedSection = await _sectionService.UpdateSectionAsync(sectionId, request);
        if (updatedSection == null)
        {
            return NotFound($"Section with ID {sectionId} not found");
        }

        return Ok(updatedSection);
    }

    /// <summary>
    /// Delete a section
    /// </summary>
    /// <param name="sectionId">The ID of the section to delete</param>
    /// <returns>Success or failure result</returns>
    [HttpDelete("sections/{sectionId}")]
    public async Task<ActionResult> DeleteSection(int sectionId)
    {
        var result = await _sectionService.DeleteSectionAsync(sectionId);
        if (!result)
        {
            return NotFound($"Section with ID {sectionId} not found");
        }

        return NoContent();
    }
} 