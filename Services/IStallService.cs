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
    Task<StallResponse?> CreateStallAsync(int vendorId, CreateStallRequest request);
    Task<StallResponse?> UploadStallPictureAsync(int stallId, IFormFile file);
    Task<IEnumerable<StallResponse>> GetStallsByProductNameAsync(string productSearchTerm);
    Task<IEnumerable<StallResponse>> SearchStallsAsync(string searchTerm);
    Task<StallResponse> UpdateDeliveryAvailabilityAsync(int stallId, bool deliveryAvailable);
    Task<PendingFeesResponse?> GetPendingFeesAsync(int stallId);
    Task<PendingFeesResponse> SubtractPendingFeesAsync(int stallId, decimal amount);
    Task AddToPendingFeesAsync(int stallId, decimal amount);
} 