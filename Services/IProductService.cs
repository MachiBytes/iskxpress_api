using iskxpress_api.DTOs.Vendors;
using Microsoft.AspNetCore.Http;

namespace iskxpress_api.Services;

public interface IProductService
{
    Task<ProductResponse?> GetProductByIdAsync(int productId);
    Task<IEnumerable<ProductResponse>> GetProductsByStallIdAsync(int stallId);
    Task<IEnumerable<ProductResponse>> GetProductsBySectionIdAsync(int sectionId);
    Task<IEnumerable<ProductResponse>> GetAvailableProductsByStallIdAsync(int stallId);
    Task<ProductResponse?> CreateProductAsync(int stallId, CreateProductRequest request);
    Task<ProductResponse?> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task<ProductResponse?> UpdateProductBasicsAsync(int productId, UpdateProductBasicsRequest request);
    Task<ProductResponse?> UpdateProductAvailabilityAsync(int productId, UpdateProductAvailabilityRequest request);
    Task<bool> DeleteProductAsync(int productId);
    Task<ProductResponse?> UploadProductPictureAsync(int productId, IFormFile file);
} 