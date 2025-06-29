using iskxpress_api.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if data already exists
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Database already seeded. Skipping seeding.");
                return;
            }

            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedStallsAsync();
            await SeedSectionsAsync();
            await SeedProductsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Seeding users...");

        var users = new List<User>
        {
            // Vendors (Google AuthProvider)
            new User
            {
                Name = "Mark Achiles Flores",
                Email = "markachilesflores2004@gmail.com",
                Verified = true,
                AuthProvider = AuthProvider.Google,
                Role = UserRole.Vendor,
                ProfilePictureId = null
            },
            
            new User
            {
                Name = "Sarah Johnson",
                Email = "sarah.johnson@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Google,
                Role = UserRole.Vendor,
                ProfilePictureId = null
            },
            
            new User
            {
                Name = "Carlos Rodriguez",
                Email = "carlos.rodriguez@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Google,
                Role = UserRole.Vendor,
                ProfilePictureId = null
            },
            
            new User
            {
                Name = "Emily Chen",
                Email = "emily.chen@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Google,
                Role = UserRole.Vendor,
                ProfilePictureId = null
            },
            
            new User
            {
                Name = "John Doe",
                Email = "john.doe@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Google,
                Role = UserRole.Vendor,
                ProfilePictureId = null
            },
            
            // Regular users (Microsoft AuthProvider)
            new User
            {
                Name = "Jane Smith",
                Email = "jane.smith@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Microsoft,
                Role = UserRole.User,
                ProfilePictureId = null
            },
            
            new User
            {
                Name = "Michael Davis",
                Email = "michael.davis@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Microsoft,
                Role = UserRole.User,
                ProfilePictureId = null
            },
            
            new User
            {
                Name = "Lisa Wilson",
                Email = "lisa.wilson@email.com",
                Verified = true,
                AuthProvider = AuthProvider.Microsoft,
                Role = UserRole.User,
                ProfilePictureId = null
            }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {users.Count} users");
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

    private async Task SeedStallsAsync()
    {
        _logger.LogInformation("Seeding stalls...");

        var vendors = await _context.Users.Where(u => u.Role == UserRole.Vendor).ToListAsync();
        var stalls = new List<Stall>();

        foreach (var vendor in vendors)
        {
            switch (vendor.Email)
            {
                case "markachilesflores2004@gmail.com":
                    stalls.Add(new Stall
                    {
                        Name = "Lola's Kitchen",
                        ShortDescription = "Authentic Filipino comfort food made with love and traditional recipes",
                        VendorId = vendor.Id,
                        PictureId = null
                    });
                    break;
                    
                case "sarah.johnson@email.com":
                    stalls.Add(new Stall
                    {
                        Name = "Burger Junction",
                        ShortDescription = "Gourmet burgers and crispy fries made with premium ingredients",
                        VendorId = vendor.Id,
                        PictureId = null
                    });
                    break;
                    
                case "carlos.rodriguez@email.com":
                    stalls.Add(new Stall
                    {
                        Name = "El Sabor Latino",
                        ShortDescription = "Fresh Mexican cuisine with bold flavors and authentic spices",
                        VendorId = vendor.Id,
                        PictureId = null
                    });
                    break;
                    
                case "emily.chen@email.com":
                    stalls.Add(new Stall
                    {
                        Name = "Golden Dragon",
                        ShortDescription = "Traditional Chinese dishes with modern presentation and fresh ingredients",
                        VendorId = vendor.Id,
                        PictureId = null
                    });
                    break;
                    
                case "john.doe@email.com":
                    stalls.Add(new Stall
                    {
                        Name = "Mama Mia Pizzeria",
                        ShortDescription = "Authentic Italian pizzas and pastas made with imported ingredients",
                        VendorId = vendor.Id,
                        PictureId = null
                    });
                    break;
            }
        }

        await _context.Stalls.AddRangeAsync(stalls);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {stalls.Count} stalls");
    }

    private async Task SeedSectionsAsync()
    {
        _logger.LogInformation("Seeding sections...");

        var stalls = await _context.Stalls.Include(s => s.Vendor).ToListAsync();
        var sections = new List<StallSection>();

        foreach (var stall in stalls)
        {
            switch (stall.Vendor.Email)
            {
                case "markachilesflores2004@gmail.com":
                    sections.AddRange(new[]
                    {
                        new StallSection { Name = "Main Dishes", StallId = stall.Id },
                        new StallSection { Name = "Rice Bowls", StallId = stall.Id },
                        new StallSection { Name = "Appetizers", StallId = stall.Id },
                        new StallSection { Name = "Desserts & Drinks", StallId = stall.Id }
                    });
                    break;
                    
                case "sarah.johnson@email.com":
                    sections.AddRange(new[]
                    {
                        new StallSection { Name = "Signature Burgers", StallId = stall.Id },
                        new StallSection { Name = "Chicken & Fish", StallId = stall.Id },
                        new StallSection { Name = "Sides & Salads", StallId = stall.Id },
                        new StallSection { Name = "Shakes & Drinks", StallId = stall.Id }
                    });
                    break;
                    
                case "carlos.rodriguez@email.com":
                    sections.AddRange(new[]
                    {
                        new StallSection { Name = "Tacos & Quesadillas", StallId = stall.Id },
                        new StallSection { Name = "Burritos & Bowls", StallId = stall.Id },
                        new StallSection { Name = "Appetizers", StallId = stall.Id },
                        new StallSection { Name = "Beverages", StallId = stall.Id }
                    });
                    break;
                    
                case "emily.chen@email.com":
                    sections.AddRange(new[]
                    {
                        new StallSection { Name = "Noodle Soups", StallId = stall.Id },
                        new StallSection { Name = "Stir Fry", StallId = stall.Id },
                        new StallSection { Name = "Dim Sum", StallId = stall.Id },
                        new StallSection { Name = "Drinks & Tea", StallId = stall.Id }
                    });
                    break;
                    
                case "john.doe@email.com":
                    sections.AddRange(new[]
                    {
                        new StallSection { Name = "Classic Pizzas", StallId = stall.Id },
                        new StallSection { Name = "Specialty Pizzas", StallId = stall.Id },
                        new StallSection { Name = "Pasta Dishes", StallId = stall.Id },
                        new StallSection { Name = "Appetizers & Desserts", StallId = stall.Id }
                    });
                    break;
            }
        }

        await _context.StallSections.AddRangeAsync(sections);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {sections.Count} sections");
    }

    private async Task SeedProductsAsync()
    {
        _logger.LogInformation("Seeding products...");

        var stalls = await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.StallSections)
            .ToListAsync();

        var categories = await _context.Categories.ToListAsync();
        var products = new List<Product>();

        foreach (var stall in stalls)
        {
            switch (stall.Vendor.Email)
            {
                case "markachilesflores2004@gmail.com":
                    await SeedLolaKitchenProducts(products, stall, categories);
                    break;
                    
                case "sarah.johnson@email.com":
                    await SeedBurgerJunctionProducts(products, stall, categories);
                    break;
                    
                case "carlos.rodriguez@email.com":
                    await SeedElSaborLatinoProducts(products, stall, categories);
                    break;
                    
                case "emily.chen@email.com":
                    await SeedGoldenDragonProducts(products, stall, categories);
                    break;
                    
                case "john.doe@email.com":
                    await SeedMamaMiaPizzeriaProducts(products, stall, categories);
                    break;
            }
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {products.Count} products");
    }

    private async Task SeedLolaKitchenProducts(List<Product> products, Stall stall, List<Category> categories)
    {
        var sections = stall.StallSections.ToList();
        var riceMealsCategory = categories.First(c => c.Name == "Rice Meals");
        var streetBitesCategory = categories.First(c => c.Name == "Street Bites");
        var dessertsCategory = categories.First(c => c.Name == "Desserts");
        var hotDrinksCategory = categories.First(c => c.Name == "Hot Drinks");

        var mainDishesSection = sections.First(s => s.Name == "Main Dishes");
        var riceBowlsSection = sections.First(s => s.Name == "Rice Bowls");
        var appetizersSection = sections.First(s => s.Name == "Appetizers");
        var dessertsSection = sections.First(s => s.Name == "Desserts & Drinks");

        products.AddRange(new[]
        {
            // Main Dishes
            new Product { Name = "Adobong Manok", BasePrice = 12.99m, PriceWithMarkup = 15.00m, PriceWithDelivery = 18.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = mainDishesSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Sinigang na Baboy", BasePrice = 14.99m, PriceWithMarkup = 17.00m, PriceWithDelivery = 20.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = mainDishesSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Kare-Kare", BasePrice = 16.99m, PriceWithMarkup = 19.00m, PriceWithDelivery = 22.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = mainDishesSection.Id, StallId = stall.Id, PictureId = null },
            
            // Rice Bowls
            new Product { Name = "Chicken Tocino Bowl", BasePrice = 9.99m, PriceWithMarkup = 11.00m, PriceWithDelivery = 14.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = riceBowlsSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Longganisa Bowl", BasePrice = 10.99m, PriceWithMarkup = 12.00m, PriceWithDelivery = 15.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = riceBowlsSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Beef Tapa Bowl", BasePrice = 11.99m, PriceWithMarkup = 14.00m, PriceWithDelivery = 17.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = riceBowlsSection.Id, StallId = stall.Id, PictureId = null },
            
            // Appetizers
            new Product { Name = "Lumpia Shanghai", BasePrice = 6.99m, PriceWithMarkup = 8.00m, PriceWithDelivery = 11.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = appetizersSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Chicken Empanada", BasePrice = 3.99m, PriceWithMarkup = 5.00m, PriceWithDelivery = 8.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = appetizersSection.Id, StallId = stall.Id, PictureId = null },
            
            // Desserts & Drinks
            new Product { Name = "Halo-Halo", BasePrice = 7.99m, PriceWithMarkup = 9.00m, PriceWithDelivery = 12.00m, Availability = ProductAvailability.Available, CategoryId = dessertsCategory.Id, SectionId = dessertsSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fresh Buko Juice", BasePrice = 3.99m, PriceWithMarkup = 5.00m, PriceWithDelivery = 8.00m, Availability = ProductAvailability.Available, CategoryId = hotDrinksCategory.Id, SectionId = dessertsSection.Id, StallId = stall.Id, PictureId = null }
        });
    }

    private async Task SeedBurgerJunctionProducts(List<Product> products, Stall stall, List<Category> categories)
    {
        var sections = stall.StallSections.ToList();
        var burgersCategory = categories.First(c => c.Name == "Burgers & Sandwiches");
        var friedSnacksCategory = categories.First(c => c.Name == "Fried Snacks");
        var icedDrinksCategory = categories.First(c => c.Name == "Iced Drinks");
        var othersCategory = categories.First(c => c.Name == "Others");

        var signatureBurgersSection = sections.First(s => s.Name == "Signature Burgers");
        var chickenFishSection = sections.First(s => s.Name == "Chicken & Fish");
        var sidesSaladsSection = sections.First(s => s.Name == "Sides & Salads");
        var shakesSection = sections.First(s => s.Name == "Shakes & Drinks");

        products.AddRange(new[]
        {
            // Signature Burgers
            new Product { Name = "Classic Beef Burger", BasePrice = 11.99m, PriceWithMarkup = 14.00m, PriceWithDelivery = 17.00m, Availability = ProductAvailability.Available, CategoryId = burgersCategory.Id, SectionId = signatureBurgersSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "BBQ Bacon Burger", BasePrice = 13.99m, PriceWithMarkup = 16.00m, PriceWithDelivery = 19.00m, Availability = ProductAvailability.Available, CategoryId = burgersCategory.Id, SectionId = signatureBurgersSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Mushroom Swiss Burger", BasePrice = 12.99m, PriceWithMarkup = 15.00m, PriceWithDelivery = 18.00m, Availability = ProductAvailability.Available, CategoryId = burgersCategory.Id, SectionId = signatureBurgersSection.Id, StallId = stall.Id, PictureId = null },
            
            // Chicken & Fish
            new Product { Name = "Crispy Chicken Burger", BasePrice = 10.99m, PriceWithMarkup = 12.00m, PriceWithDelivery = 15.00m, Availability = ProductAvailability.Available, CategoryId = burgersCategory.Id, SectionId = chickenFishSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fish & Chips", BasePrice = 13.99m, PriceWithMarkup = 16.00m, PriceWithDelivery = 19.00m, Availability = ProductAvailability.Available, CategoryId = friedSnacksCategory.Id, SectionId = chickenFishSection.Id, StallId = stall.Id, PictureId = null },
            
            // Sides & Salads
            new Product { Name = "Loaded Fries", BasePrice = 6.99m, PriceWithMarkup = 8.00m, PriceWithDelivery = 11.00m, Availability = ProductAvailability.Available, CategoryId = friedSnacksCategory.Id, SectionId = sidesSaladsSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Caesar Salad", BasePrice = 8.99m, PriceWithMarkup = 10.00m, PriceWithDelivery = 13.00m, Availability = ProductAvailability.Available, CategoryId = othersCategory.Id, SectionId = sidesSaladsSection.Id, StallId = stall.Id, PictureId = null },
            
            // Shakes & Drinks
            new Product { Name = "Chocolate Milkshake", BasePrice = 5.99m, PriceWithMarkup = 7.00m, PriceWithDelivery = 10.00m, Availability = ProductAvailability.Available, CategoryId = icedDrinksCategory.Id, SectionId = shakesSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Vanilla Milkshake", BasePrice = 5.99m, PriceWithMarkup = 7.00m, PriceWithDelivery = 10.00m, Availability = ProductAvailability.Available, CategoryId = icedDrinksCategory.Id, SectionId = shakesSection.Id, StallId = stall.Id, PictureId = null }
        });
    }

    private async Task SeedElSaborLatinoProducts(List<Product> products, Stall stall, List<Category> categories)
    {
        var sections = stall.StallSections.ToList();
        var streetBitesCategory = categories.First(c => c.Name == "Street Bites");
        var wrapsRollsCategory = categories.First(c => c.Name == "Wraps & Rolls");
        var friedSnacksCategory = categories.First(c => c.Name == "Fried Snacks");
        var icedDrinksCategory = categories.First(c => c.Name == "Iced Drinks");

        var tacosSection = sections.First(s => s.Name == "Tacos & Quesadillas");
        var burritosSection = sections.First(s => s.Name == "Burritos & Bowls");
        var appetizersSection = sections.First(s => s.Name == "Appetizers");
        var beveragesSection = sections.First(s => s.Name == "Beverages");

        products.AddRange(new[]
        {
            // Tacos & Quesadillas
            new Product { Name = "Carne Asada Tacos", BasePrice = 9.99m, PriceWithMarkup = 11.00m, PriceWithDelivery = 14.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = tacosSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Chicken Quesadilla", BasePrice = 8.99m, PriceWithMarkup = 10.00m, PriceWithDelivery = 13.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = tacosSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fish Tacos", BasePrice = 10.99m, PriceWithMarkup = 12.00m, PriceWithDelivery = 15.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = tacosSection.Id, StallId = stall.Id, PictureId = null },
            
            // Burritos & Bowls
            new Product { Name = "Beef Burrito", BasePrice = 11.99m, PriceWithMarkup = 14.00m, PriceWithDelivery = 17.00m, Availability = ProductAvailability.Available, CategoryId = wrapsRollsCategory.Id, SectionId = burritosSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Chicken Bowl", BasePrice = 10.99m, PriceWithMarkup = 12.00m, PriceWithDelivery = 15.00m, Availability = ProductAvailability.Available, CategoryId = wrapsRollsCategory.Id, SectionId = burritosSection.Id, StallId = stall.Id, PictureId = null },
            
            // Appetizers
            new Product { Name = "Loaded Nachos", BasePrice = 8.99m, PriceWithMarkup = 10.00m, PriceWithDelivery = 13.00m, Availability = ProductAvailability.Available, CategoryId = friedSnacksCategory.Id, SectionId = appetizersSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Guacamole & Chips", BasePrice = 6.99m, PriceWithMarkup = 8.00m, PriceWithDelivery = 11.00m, Availability = ProductAvailability.Available, CategoryId = friedSnacksCategory.Id, SectionId = appetizersSection.Id, StallId = stall.Id, PictureId = null },
            
            // Beverages
            new Product { Name = "Horchata", BasePrice = 3.99m, PriceWithMarkup = 5.00m, PriceWithDelivery = 8.00m, Availability = ProductAvailability.Available, CategoryId = icedDrinksCategory.Id, SectionId = beveragesSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fresh Agua Fresca", BasePrice = 3.99m, PriceWithMarkup = 5.00m, PriceWithDelivery = 8.00m, Availability = ProductAvailability.Available, CategoryId = icedDrinksCategory.Id, SectionId = beveragesSection.Id, StallId = stall.Id, PictureId = null }
        });
    }

    private async Task SeedGoldenDragonProducts(List<Product> products, Stall stall, List<Category> categories)
    {
        var sections = stall.StallSections.ToList();
        var noodlesPastaCategory = categories.First(c => c.Name == "Noodles & Pasta");
        var streetBitesCategory = categories.First(c => c.Name == "Street Bites");
        var riceMealsCategory = categories.First(c => c.Name == "Rice Meals");
        var hotDrinksCategory = categories.First(c => c.Name == "Hot Drinks");

        var noodleSoupsSection = sections.First(s => s.Name == "Noodle Soups");
        var stirFrySection = sections.First(s => s.Name == "Stir Fry");
        var dimSumSection = sections.First(s => s.Name == "Dim Sum");
        var drinksTeaSection = sections.First(s => s.Name == "Drinks & Tea");

        products.AddRange(new[]
        {
            // Noodle Soups
            new Product { Name = "Beef Noodle Soup", BasePrice = 11.99m, PriceWithMarkup = 14.00m, PriceWithDelivery = 17.00m, Availability = ProductAvailability.Available, CategoryId = noodlesPastaCategory.Id, SectionId = noodleSoupsSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Wonton Soup", BasePrice = 9.99m, PriceWithMarkup = 11.00m, PriceWithDelivery = 14.00m, Availability = ProductAvailability.Available, CategoryId = noodlesPastaCategory.Id, SectionId = noodleSoupsSection.Id, StallId = stall.Id, PictureId = null },
            
            // Stir Fry
            new Product { Name = "Kung Pao Chicken", BasePrice = 12.99m, PriceWithMarkup = 15.00m, PriceWithDelivery = 18.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = stirFrySection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Sweet & Sour Pork", BasePrice = 13.99m, PriceWithMarkup = 16.00m, PriceWithDelivery = 19.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = stirFrySection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fried Rice", BasePrice = 8.99m, PriceWithMarkup = 10.00m, PriceWithDelivery = 13.00m, Availability = ProductAvailability.Available, CategoryId = riceMealsCategory.Id, SectionId = stirFrySection.Id, StallId = stall.Id, PictureId = null },
            
            // Dim Sum
            new Product { Name = "Pork Dumplings (6pcs)", BasePrice = 7.99m, PriceWithMarkup = 9.00m, PriceWithDelivery = 12.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = dimSumSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Chicken Siu Mai (6pcs)", BasePrice = 7.99m, PriceWithMarkup = 9.00m, PriceWithDelivery = 12.00m, Availability = ProductAvailability.Available, CategoryId = streetBitesCategory.Id, SectionId = dimSumSection.Id, StallId = stall.Id, PictureId = null },
            
            // Drinks & Tea
            new Product { Name = "Jasmine Tea", BasePrice = 2.99m, PriceWithMarkup = 4.00m, PriceWithDelivery = 7.00m, Availability = ProductAvailability.Available, CategoryId = hotDrinksCategory.Id, SectionId = drinksTeaSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fresh Lychee Juice", BasePrice = 3.99m, PriceWithMarkup = 5.00m, PriceWithDelivery = 8.00m, Availability = ProductAvailability.Available, CategoryId = hotDrinksCategory.Id, SectionId = drinksTeaSection.Id, StallId = stall.Id, PictureId = null }
        });
    }

    private async Task SeedMamaMiaPizzeriaProducts(List<Product> products, Stall stall, List<Category> categories)
    {
        var sections = stall.StallSections.ToList();
        var othersCategory = categories.First(c => c.Name == "Others"); // Pizza goes to Others
        var noodlesPastaCategory = categories.First(c => c.Name == "Noodles & Pasta");
        var friedSnacksCategory = categories.First(c => c.Name == "Fried Snacks");
        var dessertsCategory = categories.First(c => c.Name == "Desserts");

        var classicPizzasSection = sections.First(s => s.Name == "Classic Pizzas");
        var specialtyPizzasSection = sections.First(s => s.Name == "Specialty Pizzas");
        var pastaDishesSection = sections.First(s => s.Name == "Pasta Dishes");
        var appetizersDessertsSection = sections.First(s => s.Name == "Appetizers & Desserts");

        products.AddRange(new[]
        {
            // Classic Pizzas
            new Product { Name = "Margherita Pizza", BasePrice = 12.99m, PriceWithMarkup = 15.00m, PriceWithDelivery = 18.00m, Availability = ProductAvailability.Available, CategoryId = othersCategory.Id, SectionId = classicPizzasSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Pepperoni Pizza", BasePrice = 14.99m, PriceWithMarkup = 17.00m, PriceWithDelivery = 20.00m, Availability = ProductAvailability.Available, CategoryId = othersCategory.Id, SectionId = classicPizzasSection.Id, StallId = stall.Id, PictureId = null },
            
            // Specialty Pizzas
            new Product { Name = "Four Cheese Pizza", BasePrice = 15.99m, PriceWithMarkup = 18.00m, PriceWithDelivery = 21.00m, Availability = ProductAvailability.Available, CategoryId = othersCategory.Id, SectionId = specialtyPizzasSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "BBQ Chicken Pizza", BasePrice = 16.99m, PriceWithMarkup = 19.00m, PriceWithDelivery = 22.00m, Availability = ProductAvailability.Available, CategoryId = othersCategory.Id, SectionId = specialtyPizzasSection.Id, StallId = stall.Id, PictureId = null },
            
            // Pasta Dishes
            new Product { Name = "Spaghetti Carbonara", BasePrice = 13.99m, PriceWithMarkup = 16.00m, PriceWithDelivery = 19.00m, Availability = ProductAvailability.Available, CategoryId = noodlesPastaCategory.Id, SectionId = pastaDishesSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Fettuccine Alfredo", BasePrice = 14.99m, PriceWithMarkup = 17.00m, PriceWithDelivery = 20.00m, Availability = ProductAvailability.Available, CategoryId = noodlesPastaCategory.Id, SectionId = pastaDishesSection.Id, StallId = stall.Id, PictureId = null },
            
            // Appetizers & Desserts
            new Product { Name = "Garlic Bread", BasePrice = 4.99m, PriceWithMarkup = 6.00m, PriceWithDelivery = 9.00m, Availability = ProductAvailability.Available, CategoryId = friedSnacksCategory.Id, SectionId = appetizersDessertsSection.Id, StallId = stall.Id, PictureId = null },
            new Product { Name = "Tiramisu", BasePrice = 8.99m, PriceWithMarkup = 10.00m, PriceWithDelivery = 13.00m, Availability = ProductAvailability.Available, CategoryId = dessertsCategory.Id, SectionId = appetizersDessertsSection.Id, StallId = stall.Id, PictureId = null }
        });
    }
} 