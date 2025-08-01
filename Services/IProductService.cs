using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using Microsoft.AspNetCore.Http;

namespace iskxpress_api.Services;

public interface IProductService
{
    Task<ProductResponse?> GetProductByIdAsync(int productId);
    Task<IEnumerable<ProductResponse>> GetProductsByStallIdAsync(int stallId);
    Task<IEnumerable<ProductResponse>> GetProductsBySectionIdAsync(int sectionId);
    Task<IEnumerable<ProductResponse>> GetAvailableProductsByStallIdAsync(int stallId);
    Task<ProductResponse> CreateAsync(int stallId, CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task<ProductResponse?> UpdateProductAvailabilityAsync(int productId, UpdateProductAvailabilityRequest request);
    Task<bool> DeleteProductAsync(int productId);
    Task<ProductResponse?> UploadProductPictureAsync(int productId, IFormFile file);
} 