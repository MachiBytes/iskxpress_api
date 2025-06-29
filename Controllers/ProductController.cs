using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Services;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products for a specific stall
    /// </summary>
    /// <param name="stallId">The ID of the stall</param>
    /// <returns>List of products from the stall</returns>
    [HttpGet("stalls/{stallId}/products")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsByStall(int stallId)
    {
        var products = await _productService.GetProductsByStallIdAsync(stallId);
        return Ok(products);
    }

    /// <summary>
    /// Get only available products for a specific stall
    /// </summary>
    /// <param name="stallId">The ID of the stall</param>
    /// <returns>List of available products from the stall</returns>
    [HttpGet("stalls/{stallId}/products/available")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAvailableProductsByStall(int stallId)
    {
        var products = await _productService.GetAvailableProductsByStallIdAsync(stallId);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product for a stall
    /// </summary>
    /// <param name="stallId">The ID of the stall</param>
    /// <param name="request">The product information</param>
    /// <returns>The created product information</returns>
    [HttpPost("stalls/{stallId}/products")]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        int stallId,
        [FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdProduct = await _productService.CreateProductAsync(stallId, request);
        if (createdProduct == null)
        {
            return BadRequest($"Unable to create product. Check that stall {stallId} exists, and that the section and category belong to the stall.");
        }

        return CreatedAtAction(nameof(GetProduct), new { productId = createdProduct.Id }, createdProduct);
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    /// <param name="productId">The ID of the product</param>
    /// <returns>The product information</returns>
    [HttpGet("products/{productId}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return NotFound($"Product with ID {productId} not found");
        }

        return Ok(product);
    }

    /// <summary>
    /// Get all products for a specific section
    /// </summary>
    /// <param name="sectionId">The ID of the section</param>
    /// <returns>List of products in the section</returns>
    [HttpGet("products/section/{sectionId}")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsBySection(int sectionId)
    {
        var products = await _productService.GetProductsBySectionIdAsync(sectionId);
        return Ok(products);
    }

    /// <summary>
    /// Update a product
    /// </summary>
    /// <param name="productId">The ID of the product to update</param>
    /// <param name="request">The updated product information</param>
    /// <returns>The updated product information</returns>
    [HttpPut("products/{productId}")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        int productId,
        [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedProduct = await _productService.UpdateProductAsync(productId, request);
        if (updatedProduct == null)
        {
            return NotFound($"Product with ID {productId} not found");
        }

        return Ok(updatedProduct);
    }

    /// <summary>
    /// Update basic product information (name, picture, base price only)
    /// </summary>
    /// <param name="productId">The ID of the product to update</param>
    /// <param name="request">The basic product information to update</param>
    /// <returns>The updated product information</returns>
    [HttpPatch("products/{productId}/basics")]
    public async Task<ActionResult<ProductResponse>> UpdateProductBasics(
        int productId,
        [FromBody] UpdateProductBasicsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedProduct = await _productService.UpdateProductBasicsAsync(productId, request);
        if (updatedProduct == null)
        {
            return NotFound($"Product with ID {productId} not found");
        }

        return Ok(updatedProduct);
    }

    /// <summary>
    /// Update product availability
    /// </summary>
    /// <param name="productId">The ID of the product to update</param>
    /// <param name="request">The availability update request</param>
    /// <returns>The updated product information</returns>
    [HttpPatch("products/{productId}/availability")]
    public async Task<ActionResult<ProductResponse>> UpdateProductAvailability(
        int productId,
        [FromBody] UpdateProductAvailabilityRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedProduct = await _productService.UpdateProductAvailabilityAsync(productId, request);
        if (updatedProduct == null)
        {
            return NotFound($"Product with ID {productId} not found");
        }

        return Ok(updatedProduct);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="productId">The ID of the product to delete</param>
    /// <returns>Success or failure result</returns>
    [HttpDelete("products/{productId}")]
    public async Task<ActionResult> DeleteProduct(int productId)
    {
        var result = await _productService.DeleteProductAsync(productId);
        if (!result)
        {
            return NotFound($"Product with ID {productId} not found");
        }

        return NoContent();
    }
} 