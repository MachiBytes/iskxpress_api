using Microsoft.AspNetCore.Mvc;
using iskxpress_api.Repositories;
using iskxpress_api.Models;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all categories
    /// </summary>
    /// <returns>List of all categories</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(categories.OrderBy(c => c.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }
} 