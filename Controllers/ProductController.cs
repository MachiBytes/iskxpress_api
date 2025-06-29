using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Services;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
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

        try
        {
            var createdProduct = await _productService.CreateAsync(stallId, request);
            if (createdProduct == null)
            {
                return BadRequest($"Unable to create product. Check that stall {stallId} exists, and that the section and category belong to the stall.");
            }
            return CreatedAtAction(nameof(GetProduct), new { productId = createdProduct.Id }, createdProduct);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product for stall {StallId}", stallId);
            return StatusCode(500, "Internal server error");
        }
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

        try
        {
            var updatedProduct = await _productService.UpdateAsync(productId, request);
            return Ok(updatedProduct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
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

    /// <summary>
    /// Upload or replace product picture
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="file">The image file to upload</param>
    /// <returns>Updated product with new picture</returns>
    /// <response code="200">Product picture uploaded successfully</response>
    /// <response code="400">Invalid file or request</response>
    /// <response code="404">Product not found</response>
    /// <response code="413">File too large</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("products/{productId}/upload-picture")]
    [ProducesResponseType(typeof(ProductResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(413)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ProductResponse>> UploadProductPicture(int productId, IFormFile file)
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

            // Check if product exists
            var existingProduct = await _productService.GetProductByIdAsync(productId);
            if (existingProduct == null)
                return NotFound($"Product with ID {productId} not found");

            // Upload the file (this will automatically replace any existing product picture)
            var updatedProduct = await _productService.UploadProductPictureAsync(productId, file);
            if (updatedProduct == null)
                return NotFound($"Product with ID {productId} not found");

            return Ok(updatedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading product picture for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }
} 