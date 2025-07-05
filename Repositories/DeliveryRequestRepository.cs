using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for delivery request operations
/// </summary>
public class DeliveryRequestRepository : GenericRepository<DeliveryRequest>, IDeliveryRequestRepository
{
    public DeliveryRequestRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DeliveryRequest>> GetByStatusAsync(DeliveryRequestStatus status)
    {
        return await _context.DeliveryRequests
            .Include(dr => dr.Order)
                .ThenInclude(o => o.User)
            .Include(dr => dr.Order)
                .ThenInclude(o => o.Stall)
            .Include(dr => dr.AssignedDeliveryPartner)
            .Where(dr => dr.Status == status)
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();
    }

    public async Task<DeliveryRequest?> GetByOrderIdAsync(int orderId)
    {
        return await _context.DeliveryRequests
            .Include(dr => dr.Order)
                .ThenInclude(o => o.User)
            .Include(dr => dr.Order)
                .ThenInclude(o => o.Stall)
            .Include(dr => dr.AssignedDeliveryPartner)
            .FirstOrDefaultAsync(dr => dr.OrderId == orderId);
    }

    public async Task<IEnumerable<DeliveryRequest>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId)
    {
        return await _context.DeliveryRequests
            .Include(dr => dr.Order)
                .ThenInclude(o => o.User)
            .Include(dr => dr.Order)
                .ThenInclude(o => o.Stall)
            .Include(dr => dr.AssignedDeliveryPartner)
            .Where(dr => dr.AssignedDeliveryPartnerId == deliveryPartnerId)
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeliveryRequest>> GetPendingRequestsAsync()
    {
        return await _context.DeliveryRequests
            .Include(dr => dr.Order)
                .ThenInclude(o => o.User)
            .Include(dr => dr.Order)
                .ThenInclude(o => o.Stall)
            .Include(dr => dr.AssignedDeliveryPartner)
            .Where(dr => dr.Status == DeliveryRequestStatus.Pending)
            .OrderBy(dr => dr.CreatedAt) // Oldest first for fair assignment
            .ToListAsync();
    }

    public async Task<IEnumerable<DeliveryRequest>> GetAllWithDetailsAsync()
    {
        return await _context.DeliveryRequests
            .Include(dr => dr.Order)
                .ThenInclude(o => o.User)
            .Include(dr => dr.Order)
                .ThenInclude(o => o.Stall)
            .Include(dr => dr.AssignedDeliveryPartner)
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();
    }

    public async Task<DeliveryRequest?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.DeliveryRequests
            .Include(dr => dr.Order)
                .ThenInclude(o => o.User)
            .Include(dr => dr.Order)
                .ThenInclude(o => o.Stall)
            .Include(dr => dr.AssignedDeliveryPartner)
            .FirstOrDefaultAsync(dr => dr.Id == id);
    }
} 