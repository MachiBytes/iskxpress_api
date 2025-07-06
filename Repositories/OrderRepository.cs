using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for order operations
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, OrderStatus? status = null)
    {
        var query = _context.Orders
            .Include(o => o.Stall)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Where(o => o.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetStallOrdersAsync(int stallId, OrderStatus? status = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Stall)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Where(o => o.StallId == stallId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Stall)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByStallIdAsync(int stallId)
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Where(o => o.StallId == stallId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId)
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Where(o => o.DeliveryPartnerId == deliveryPartnerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u.ProfilePicture)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.Stall)
                .ThenInclude(s => s.Picture)
            .Include(o => o.DeliveryPartner)
                .ThenInclude(dp => dp.ProfilePicture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetWithItemsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.Stall)
            .ThenInclude(s => s.Vendor)
            .Include(o => o.DeliveryPartner)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Picture)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> GetByVendorOrderIdAsync(string vendorOrderId)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.Stall)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Picture)
            .FirstOrDefaultAsync(o => o.VendorOrderId == vendorOrderId);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync(bool? hasDeliveryPartner = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Stall)
            .Include(o => o.DeliveryPartner)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .AsQueryable();

        if (hasDeliveryPartner.HasValue)
        {
            if (hasDeliveryPartner.Value)
            {
                query = query.Where(o => o.DeliveryPartnerId != null);
            }
            else
            {
                query = query.Where(o => o.DeliveryPartnerId == null);
            }
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetStallOrdersWithDeliveryPartnerAsync(int stallId)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.DeliveryPartner)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Where(o => o.StallId == stallId && o.DeliveryPartnerId != null)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetDeliveryPartnerOrdersAsync(int deliveryPartnerId, bool? isFinished = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Stall)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Picture)
            .Where(o => o.DeliveryPartnerId == deliveryPartnerId);

        if (isFinished.HasValue)
        {
            if (isFinished.Value)
            {
                // Finished orders: Accomplished or Rejected
                query = query.Where(o => o.Status == OrderStatus.Accomplished || o.Status == OrderStatus.Rejected);
            }
            else
            {
                // Ongoing orders: All statuses except Accomplished and Rejected
                query = query.Where(o => o.Status != OrderStatus.Accomplished && o.Status != OrderStatus.Rejected);
            }
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
} 