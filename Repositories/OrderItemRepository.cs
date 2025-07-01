using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for order item operations
/// </summary>
public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.OrderId == orderId)
            .OrderBy(oi => oi.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.ProductId == productId)
            .OrderBy(oi => oi.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderItem>> GetAllWithDetailsAsync()
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .OrderBy(oi => oi.Id)
            .ToListAsync();
    }

    public async Task<OrderItem?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .FirstOrDefaultAsync(oi => oi.Id == id);
    }
} 