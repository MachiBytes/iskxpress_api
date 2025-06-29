using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services.Mapping;
using Microsoft.AspNetCore.Http;

namespace iskxpress_api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IStallRepository _stallRepository;
    private readonly IStallSectionRepository _sectionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileRepository _fileRepository;

    public ProductService(
        IProductRepository productRepository,
        IStallRepository stallRepository,
        IStallSectionRepository sectionRepository,
        ICategoryRepository categoryRepository,
        IFileRepository fileRepository)
    {
        _productRepository = productRepository;
        _stallRepository = stallRepository;
        _sectionRepository = sectionRepository;
        _categoryRepository = categoryRepository;
        _fileRepository = fileRepository;
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

    public async Task<ProductResponse?> CreateProductAsync(int stallId, CreateProductRequest request)
    {
        // Verify that the stall exists
        var stall = await _stallRepository.GetByIdAsync(stallId);
        if (stall == null)
        {
            return null; // Stall doesn't exist
        }

        // Verify that the section belongs to the stall
        var section = await _sectionRepository.GetByIdAsync(request.SectionId);
        if (section == null || section.StallId != stallId)
        {
            return null; // Section doesn't belong to this stall
        }

        // Verify that the category exists and belongs to the same vendor as the stall
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null || category.VendorId != stall.VendorId)
        {
            return null; // Category doesn't belong to the stall's vendor
        }

        var newProduct = new Product
        {
            Name = request.Name,
            PictureId = request.PictureId,
            BasePrice = request.BasePrice,
            PriceWithMarkup = CalculateMarkupPrice(request.BasePrice),
            PriceWithDelivery = CalculateDeliveryPrice(request.BasePrice),
            Availability = request.Availability,
            CategoryId = request.CategoryId,
            SectionId = request.SectionId,
            StallId = stallId
        };

        var createdProduct = await _productRepository.AddAsync(newProduct);
        
        // Load related data for the response
        var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(createdProduct.Id);
        
        return productWithDetails?.ToProductResponse();
    }

    public async Task<ProductResponse?> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        if (product == null)
        {
            return null;
        }

        // Verify that the section belongs to the same stall as the product
        var section = await _sectionRepository.GetByIdAsync(request.SectionId);
        if (section == null || section.StallId != product.StallId)
        {
            return null; // Section doesn't belong to the same stall
        }

        // Verify that the category belongs to the same vendor as the product's stall
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null || category.VendorId != product.Stall.VendorId)
        {
            return null; // Category doesn't belong to the same vendor
        }

        // Update product properties
        product.Name = request.Name;
        product.PictureId = request.PictureId;
        product.BasePrice = request.BasePrice;
        product.PriceWithMarkup = CalculateMarkupPrice(request.BasePrice);
        product.PriceWithDelivery = CalculateDeliveryPrice(request.BasePrice);
        product.Availability = request.Availability;
        product.CategoryId = request.CategoryId;
        product.SectionId = request.SectionId;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        
        // Load related data for the response
        var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(updatedProduct.Id);
        
        return productWithDetails?.ToProductResponse();
    }

    public async Task<ProductResponse?> UpdateProductBasicsAsync(int productId, UpdateProductBasicsRequest request)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        if (product == null)
        {
            return null;
        }

        // Update only basic properties
        product.Name = request.Name;
        product.PictureId = request.PictureId;
        product.BasePrice = request.BasePrice;
        product.PriceWithMarkup = CalculateMarkupPrice(request.BasePrice);
        product.PriceWithDelivery = CalculateDeliveryPrice(request.BasePrice);

        var updatedProduct = await _productRepository.UpdateAsync(product);
        
        // Load related data for the response
        var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(updatedProduct.Id);
        
        return productWithDetails?.ToProductResponse();
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
    /// Calculates the markup price by adding 10% to the base price and rounding up using Math.Ceiling
    /// </summary>
    private static decimal CalculateMarkupPrice(decimal basePrice)
    {
        return Math.Ceiling(basePrice * 1.10m);
    }

    /// <summary>
    /// Calculates the delivery price by adding $3.00 to the markup price
    /// </summary>
    private static decimal CalculateDeliveryPrice(decimal basePrice)
    {
        return CalculateMarkupPrice(basePrice) + 3.00m;
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