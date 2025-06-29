using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using Xunit;

namespace iskxpress_api.Tests;

public class CategoryRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IskExpressDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var category1 = new Category { Name = "Rice Meals" };
        var category2 = new Category { Name = "Fried Snacks" };
        var category3 = new Category { Name = "Street Bites" };

        _context.Categories.AddRange(category1, category2, category3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(result, c => c.Name == "Rice Meals");
        Assert.Contains(result, c => c.Name == "Fried Snacks");
        Assert.Contains(result, c => c.Name == "Street Bites");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectCategory()
    {
        // Arrange
        var category = new Category { Name = "Desserts" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Desserts", result.Name);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 