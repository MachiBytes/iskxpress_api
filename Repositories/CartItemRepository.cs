using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for cart item operations
/// </summary>
public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId)
    {
        return await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .OrderBy(ci => ci.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<CartItem>> GetByProductIdAsync(int productId)
    {
        return await _context.CartItems
            .Where(ci => ci.ProductId == productId)
            .OrderBy(ci => ci.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<CartItem>> GetByStallIdAsync(int stallId)
    {
        return await _context.CartItems
            .Where(ci => ci.StallId == stallId)
            .OrderBy(ci => ci.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<CartItem>> GetByUserAndStallAsync(int userId, int stallId)
    {
        return await _context.CartItems
            .Where(ci => ci.UserId == userId && ci.StallId == stallId)
            .OrderBy(ci => ci.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<CartItem>> GetAllWithDetailsAsync()
    {
        return await _context.CartItems
            .Include(ci => ci.User)
            .Include(ci => ci.Product)
            .Include(ci => ci.Stall)
            .OrderBy(ci => ci.Id)
            .ToListAsync();
    }

    public async Task<CartItem?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.CartItems
            .Include(ci => ci.User)
            .Include(ci => ci.Product)
            .Include(ci => ci.Stall)
            .FirstOrDefaultAsync(ci => ci.Id == id);
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (cartItems.Any())
        {
            _context.CartItems.RemoveRange(cartItems);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        return true; // Cart was already empty
    }
} 