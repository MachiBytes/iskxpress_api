using iskxpress_api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace iskxpress_api.Data;

public class DatabaseSeeder
{
    private readonly IskExpressDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IskExpressDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds production data (categories only)
    /// </summary>
    public async Task SeedProductionAsync()
    {
        try
        {
            _logger.LogInformation("Starting production database seeding...");
            
            await SeedCategoriesAsync();
            
            _logger.LogInformation("Production database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding production database");
            throw;
        }
    }

    /// <summary>
    /// Seeds development data (categories and all stalls/products from JSON)
    /// </summary>
    public async Task SeedDevelopmentAsync()
    {
        try
        {
            _logger.LogInformation("Starting development database seeding...");

            await SeedCategoriesAsync();
            await SeedStallsFromJsonAsync();

            _logger.LogInformation("Development database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding development database");
            throw;
        }
    }

    /// <summary>
    /// Legacy method for backward compatibility - calls development seeding
    /// </summary>
    [Obsolete("Use SeedDevelopmentAsync() or SeedProductionAsync() instead")]
    public async Task SeedAsync()
    {
        await SeedDevelopmentAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        _logger.LogInformation("Seeding categories...");

        // Check if categories already exist
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist. Skipping category seeding.");
            return;
        }

        // Seed the global categories (like enums)
        var categories = new List<Category>
        {
            new Category { Name = "Rice Meals" },
            new Category { Name = "Fried Snacks" },
            new Category { Name = "Street Bites" },
            new Category { Name = "Noodles & Pasta" },
            new Category { Name = "Burgers & Sandwiches" },
            new Category { Name = "Wraps & Rolls" },
            new Category { Name = "Iced Drinks" },
            new Category { Name = "Hot Drinks" },
            new Category { Name = "Bottled Drinks" },
            new Category { Name = "Desserts" },
            new Category { Name = "Others" }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {categories.Count} global categories");
    }

    /// <summary>
    /// Seeds stalls from JSON file in a production-safe way
    /// Only adds stalls that don't already exist
    /// </summary>
    public async Task SeedStallsFromJsonAsync()
    {
        try
        {
            _logger.LogInformation("Starting stalls seeding from JSON...");

            // Load JSON data
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "JSON", "stalls.json");
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning($"Stalls JSON file not found at {jsonPath}");
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var stallsData = JsonSerializer.Deserialize<List<StallJsonData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (stallsData == null || !stallsData.Any())
            {
                _logger.LogWarning("No stalls data found in JSON file");
                return;
            }

            // Get existing stalls to avoid duplicates
            var existingStalls = await _context.Stalls
                .Include(s => s.Vendor)
                .ToListAsync();

            var existingStallNames = existingStalls.Select(s => s.Name.ToLower()).ToHashSet();

            var stallsCreated = 0;
            foreach (var stallData in stallsData)
            {
                // Skip if stall already exists
                if (existingStallNames.Contains(stallData.StallName.ToLower()))
                {
                    _logger.LogInformation($"Stall '{stallData.StallName}' already exists, skipping...");
                    continue;
                }

                await CreateStallFromJsonDataAsync(stallData);
                stallsCreated++;
            }

            if (stallsCreated > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Stalls seeding from JSON completed successfully! Created {stallsCreated} new stalls.");
            }
            else
            {
                _logger.LogInformation("No new stalls were created - all stalls from JSON already exist.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding stalls from JSON");
            throw;
        }
    }

    private async Task CreateStallFromJsonDataAsync(StallJsonData stallData)
    {
        _logger.LogInformation($"Creating stall: {stallData.StallName}");

        // 1. Create or get user (vendor)
        var user = await GetOrCreateUserAsync(stallData.Vendor, stallData.StallName);

        // 2. Create stall avatar file record
        var stallAvatarFile = await CreateFileRecordFromUrlAsync(
            stallData.Avatar, 
            FileType.StallAvatar, 
            $"stall_avatars/{stallData.StallName.Replace(" ", "_").ToUpper()}.png"
        );

        // 3. Create stall
        var stall = new Stall
        {
            Name = stallData.StallName,
            StallNumber = int.TryParse(stallData.StallNumber, out var stallNum) ? stallNum : 0,
            ShortDescription = stallData.Description,
            VendorId = user.Id,
            PictureId = stallAvatarFile?.Id,
            DeliveryAvailable = false,
            PendingFees = 0.00m
        };

        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync(); // Save to get the stall ID

        // 4. Create default "Products" section
        var defaultSection = new StallSection
        {
            Name = "Products",
            StallId = stall.Id
        };

        _context.StallSections.Add(defaultSection);
        await _context.SaveChangesAsync(); // Save to get the section ID

        // 5. Create products
        await CreateProductsForStallAsync(stallData.Products, stall.Id, defaultSection.Id);

        _logger.LogInformation($"Successfully created stall '{stallData.StallName}' with {stallData.Products.Count} products");
    }

    private async Task<User> GetOrCreateUserAsync(string email, string stallName)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (existingUser != null)
        {
            _logger.LogInformation($"User with email {email} already exists, using existing user");
            return existingUser;
        }

        // Create new user
        var user = new User
        {
            Name = stallName, // Use stall name as user name
            Email = email,
            Premium = false,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            ProfilePictureId = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // Save to get the user ID

        _logger.LogInformation($"Created new user for vendor: {email}");
        return user;
    }

    private async Task<FileRecord?> CreateFileRecordFromUrlAsync(string url, FileType fileType, string objectKey)
    {
        if (string.IsNullOrEmpty(url))
        {
            _logger.LogWarning($"URL is null or empty for {fileType}");
            return null;
        }

        try
        {
            // Extract filename from URL
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);

            var fileRecord = new FileRecord
            {
                Type = fileType,
                ObjectKey = objectKey,
                ObjectUrl = url,
                OriginalFileName = fileName,
                ContentType = GetContentTypeFromExtension(fileName),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Files.Add(fileRecord);
            await _context.SaveChangesAsync(); // Save to get the file ID

            _logger.LogInformation($"Created file record for {fileType}: {objectKey}");
            return fileRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create file record for URL: {url}");
            return null;
        }
    }

    private async Task CreateProductsForStallAsync(List<ProductJsonData> products, int stallId, int sectionId)
    {
        // Get all categories
        var categories = await _context.Categories.ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var productData in products)
        {
            // Get or create category
            if (!categoryDict.TryGetValue(productData.Category, out var categoryId))
            {
                _logger.LogWarning($"Category '{productData.Category}' not found, using 'Others' category");
                categoryId = categoryDict.GetValueOrDefault("Others", categories.First().Id);
            }

            // Create product image file record
            var productImageFile = await CreateFileRecordFromUrlAsync(
                productData.Picture,
                FileType.ProductImage,
                $"product_pictures/{productData.Name.Replace(" ", "%20").ToUpper()}.jpg"
            );

            var markup = Math.Ceiling(productData.Price * 1.1m);
            var premiumUserPrice = Math.Ceiling(productData.Price * 1.1m * 0.90m);
            // Create product
            var product = new Product
            {
                Name = productData.Name,
                BasePrice = productData.Price,
                PriceWithMarkup = markup,
                PremiumUserPrice = premiumUserPrice,
                CategoryId = categoryId,
                SectionId = sectionId,
                StallId = stallId,
                PictureId = productImageFile?.Id,
                Availability = ProductAvailability.Available
            };

            _context.Products.Add(product);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Created {products.Count} products for stall {stallId}");
    }

    private string? GetContentTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    // JSON data classes for deserialization
    private class StallJsonData
    {
        [JsonPropertyName("stall_name")]
        public string StallName { get; set; } = string.Empty;
        
        [JsonPropertyName("stall_number")]
        public string StallNumber { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public List<ProductJsonData> Products { get; set; } = new();
    }

    private class ProductJsonData
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Section { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }
} 