using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services.Mapping;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace iskxpress_api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IStallRepository _stallRepository;
    private readonly IStallSectionRepository _sectionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IStallRepository stallRepository,
        IStallSectionRepository sectionRepository,
        ICategoryRepository categoryRepository,
        IFileRepository fileRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _stallRepository = stallRepository;
        _sectionRepository = sectionRepository;
        _categoryRepository = categoryRepository;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task<ProductResponse?> GetProductByIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        return product?.ToProductResponse();
    }

    public async Task<IEnumerable<ProductResponse>> GetProductsByStallIdAsync(int stallId)
    {
        var products = await _productRepository.GetByStallIdAsync(stallId);
        return products.Select(p => p.ToProductResponse());
    }

    public async Task<IEnumerable<ProductResponse>> GetProductsBySectionIdAsync(int sectionId)
    {
        var products = await _productRepository.GetBySectionIdAsync(sectionId);
        return products.Select(p => p.ToProductResponse());
    }

    public async Task<IEnumerable<ProductResponse>> GetAvailableProductsByStallIdAsync(int stallId)
    {
        var products = await _productRepository.GetByStallIdAsync(stallId);
        return products.Where(p => p.Availability == ProductAvailability.Available)
                      .Select(p => p.ToProductResponse());
    }

    public async Task<ProductResponse> CreateAsync(int stallId, CreateProductRequest request)
    {
        try
        {
            // Get the stall to validate it exists
            var stall = await _stallRepository.GetByIdAsync(stallId);
            if (stall == null)
            {
                throw new ArgumentException($"Stall with ID {stallId} not found");
            }

            // Validate that the category exists (no need to check VendorId since categories are global)
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {request.CategoryId} not found");
            }

            // Validate that the section belongs to the stall
            var section = await _sectionRepository.GetByIdAsync(request.SectionId);
            if (section == null || section.StallId != stallId)
            {
                throw new ArgumentException($"Section with ID {request.SectionId} not found or does not belong to the specified stall");
            }

            var product = request.ToEntity();
            product.StallId = stallId;

            var createdProduct = await _productRepository.AddAsync(product);
            var response = createdProduct.ToResponse();

            _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            // Validate that the category exists (no need to check VendorId since categories are global)
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {request.CategoryId} not found");
            }

            // Validate that the section belongs to the same stall as the product
            var section = await _sectionRepository.GetByIdAsync(request.SectionId);
            if (section == null || section.StallId != existingProduct.StallId)
            {
                throw new ArgumentException($"Section with ID {request.SectionId} not found or does not belong to the product's stall");
            }

            // Update the product with new values
            existingProduct.Name = request.Name;
            existingProduct.BasePrice = request.BasePrice;
            existingProduct.PriceWithMarkup = CalculateMarkupPrice(request.BasePrice);
            existingProduct.PriceWithDelivery = CalculateDeliveryPrice(request.BasePrice);
            existingProduct.CategoryId = request.CategoryId;
            existingProduct.SectionId = request.SectionId;
            // PictureId stays the same (can be updated via upload endpoint)

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            var response = updatedProduct.ToResponse();

            _logger.LogInformation("Product updated successfully with ID: {ProductId}", updatedProduct.Id);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            throw;
        }
    }



    public async Task<ProductResponse?> UpdateProductAvailabilityAsync(int productId, UpdateProductAvailabilityRequest request)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        if (product == null)
        {
            return null;
        }

        // Update only availability
        product.Availability = request.Availability;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        
        // Load related data for the response
        var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(updatedProduct.Id);
        
        return productWithDetails?.ToProductResponse();
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return false;
        }

        return await _productRepository.DeleteAsync(productId);
    }

    /// <summary>
    /// Calculates the markup price by adding 5% to the base price
    /// </summary>
    private static decimal CalculateMarkupPrice(decimal basePrice)
    {
        return basePrice + (basePrice * 0.05m);
    }

    /// <summary>
    /// Calculates the delivery price by adding ₱10.00 to the markup price and rounding up
    /// </summary>
    private static decimal CalculateDeliveryPrice(decimal basePrice)
    {
        var markupPrice = CalculateMarkupPrice(basePrice);
        return Math.Ceiling(markupPrice + 10.00m);
    }

    public async Task<ProductResponse?> UploadProductPictureAsync(int productId, IFormFile file)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        if (product == null)
        {
            return null;
        }

        // Get file extension from the original filename
        var fileExtension = Path.GetExtension(file.FileName)?.TrimStart('.').ToLowerInvariant() ?? "jpg";

        // Upload the file using FileRepository (this automatically replaces existing files)
        using var fileStream = file.OpenReadStream();
        var fileRecord = await _fileRepository.UploadFileAsync(
            FileType.ProductImage,
            productId,
            fileStream,
            file.ContentType,
            file.FileName,
            fileExtension
        );

        // Update product with new picture reference
        product.PictureId = fileRecord.Id;
        var updatedProduct = await _productRepository.UpdateAsync(product);

        // Load related data for the response
        var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(updatedProduct.Id);
        
        return productWithDetails?.ToProductResponse();
    }
} 