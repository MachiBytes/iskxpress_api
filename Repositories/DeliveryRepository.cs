using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for delivery operations
/// </summary>
public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
{
    public DeliveryRepository(IskExpressDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Delivery>> GetAllAsync()
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .ToListAsync();
    }

    public override async Task<Delivery?> GetByIdAsync(int id)
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <summary>
    /// Gets deliveries by order ID
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Collection of deliveries for the order</returns>
    public async Task<IEnumerable<Delivery>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Deliveries
            .Where(d => d.OrderId == orderId)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .ToListAsync();
    }

    /// <summary>
    /// Gets deliveries by delivery partner ID
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>Collection of deliveries for the partner</returns>
    public async Task<IEnumerable<Delivery>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId)
    {
        return await _context.Deliveries
            .Where(d => d.DeliveryPartnerId == deliveryPartnerId)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .ToListAsync();
    }

    /// <summary>
    /// Gets deliveries by status
    /// </summary>
    /// <param name="status">The delivery status</param>
    /// <returns>Collection of deliveries with the specified status</returns>
    public async Task<IEnumerable<Delivery>> GetByStatusAsync(DeliveryStatus status)
    {
        return await _context.Deliveries
            .Where(d => d.DeliveryStatus == status)
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .ToListAsync();
    }

    /// <summary>
    /// Gets deliveries with their related order and delivery partner information
    /// </summary>
    /// <returns>Collection of deliveries with navigation properties loaded</returns>
    public async Task<IEnumerable<Delivery>> GetAllWithDetailsAsync()
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a delivery by ID with related order and delivery partner information
    /// </summary>
    /// <param name="id">The delivery ID</param>
    /// <returns>The delivery with navigation properties loaded, or null if not found</returns>
    public async Task<Delivery?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Deliveries
            .Include(d => d.Order)
            .Include(d => d.DeliveryPartner)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
} 