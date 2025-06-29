using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services.Mapping;

namespace iskxpress_api.Services;

public class StallService : IStallService
{
    private readonly IStallRepository _stallRepository;

    public StallService(IStallRepository stallRepository)
    {
        _stallRepository = stallRepository;
    }

    public async Task<IEnumerable<StallResponse>> GetAllStallsAsync()
    {
        var stalls = await _stallRepository.GetAllWithDetailsAsync();
        return stalls.Select(s => s.ToStallResponse());
    }

    public async Task<StallResponse?> GetStallByIdAsync(int stallId)
    {
        var stall = await _stallRepository.GetByIdWithDetailsAsync(stallId);
        return stall?.ToStallResponse();
    }

    public async Task<StallResponse?> GetStallByVendorIdAsync(int vendorId)
    {
        var stall = await _stallRepository.GetByVendorIdAsync(vendorId);
        return stall?.ToStallResponse();
    }

    public async Task<StallResponse?> UpdateStallAsync(int stallId, UpdateStallRequest request)
    {
        var stall = await _stallRepository.GetByIdAsync(stallId);
        if (stall == null)
        {
            return null;
        }

        stall.Name = request.Name;
        stall.ShortDescription = request.ShortDescription;
        stall.PictureId = request.PictureId;

        var updatedStall = await _stallRepository.UpdateAsync(stall);
        return updatedStall.ToStallResponse();
    }

    public async Task<StallResponse?> CreateStallAsync(CreateStallRequest request)
    {
        // Check if vendor already has a stall
        var existingStall = await _stallRepository.GetByVendorIdAsync(request.VendorId);
        if (existingStall != null)
        {
            return null; // Vendor already has a stall
        }

        var newStall = new Stall
        {
            Name = request.Name,
            ShortDescription = request.ShortDescription,
            PictureId = request.PictureId,
            VendorId = request.VendorId
        };

        var createdStall = await _stallRepository.AddAsync(newStall);
        return createdStall.ToStallResponse();
    }
} 