using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for order confirmation operations
/// </summary>
public class OrderConfirmationRepository : GenericRepository<OrderConfirmation>, IOrderConfirmationRepository
{
    public OrderConfirmationRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<OrderConfirmation?> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderConfirmations
            .Include(oc => oc.Order)
                .ThenInclude(o => o.User)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.Stall)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.DeliveryPartner)
            .FirstOrDefaultAsync(oc => oc.OrderId == orderId);
    }

    public async Task<IEnumerable<OrderConfirmation>> GetPendingAutoConfirmationsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.OrderConfirmations
            .Include(oc => oc.Order)
                .ThenInclude(o => o.User)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.Stall)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.DeliveryPartner)
            .Where(oc => !oc.IsConfirmed && !oc.IsAutoConfirmed && oc.ConfirmationDeadline <= now)
            .OrderBy(oc => oc.ConfirmationDeadline) // Process oldest first
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderConfirmation>> GetAllWithDetailsAsync()
    {
        return await _context.OrderConfirmations
            .Include(oc => oc.Order)
                .ThenInclude(o => o.User)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.Stall)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.DeliveryPartner)
            .OrderByDescending(oc => oc.CreatedAt)
            .ToListAsync();
    }

    public async Task<OrderConfirmation?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.OrderConfirmations
            .Include(oc => oc.Order)
                .ThenInclude(o => o.User)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.Stall)
            .Include(oc => oc.Order)
                .ThenInclude(o => o.DeliveryPartner)
            .FirstOrDefaultAsync(oc => oc.Id == id);
    }
} 