using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using Microsoft.AspNetCore.Http;

namespace iskxpress_api.Services;

public interface IStallService
{
    Task<IEnumerable<StallResponse>> GetAllStallsAsync();
    Task<StallResponse?> GetStallByIdAsync(int stallId);
    Task<StallResponse?> GetStallByVendorIdAsync(int vendorId);
    Task<StallResponse?> UpdateStallAsync(int stallId, UpdateStallRequest request);
    Task<StallResponse?> CreateStallAsync(CreateStallRequest request);
    Task<StallResponse?> UploadStallPictureAsync(int stallId, IFormFile file);
} 