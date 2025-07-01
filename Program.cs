using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using iskxpress_api.Data;
using iskxpress_api.Repositories;
using iskxpress_api.Services;
using Amazon.S3;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ISK Express API",
        Version = "v1",
        Description = "A comprehensive API for ISK Express food delivery platform with Firebase integration",
        Contact = new OpenApiContact
        {
            Name = "ISK Express Team",
            Email = "support@iskexpress.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    // Add authorization documentation
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
});

// Add controllers with improved JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Add health checks
builder.Services.AddHealthChecks();

// Add Entity Framework
builder.Services.AddDbContext<IskExpressDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStallRepository, StallRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStallSectionRepository, StallSectionRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

// Register AWS S3 services
builder.Services.AddScoped<IAmazonS3>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var region = Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"] ?? "us-east-1");
    return new Amazon.S3.AmazonS3Client(region);
});
builder.Services.AddScoped<IS3Repository, S3Repository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStallService, StallService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();

// Register seeder
builder.Services.AddScoped<DatabaseSeeder>();

// Add CORS - Allow everything for temporary private use
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add request/response logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

// Seed database - Categories in all environments, full data only in development
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<IskExpressDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        if (app.Environment.IsDevelopment())
        {
            // Seed full development data (users, stalls, products, etc.)
            logger.LogInformation("Development environment detected - seeding full test data");
            await seeder.SeedDevelopmentAsync();
        }
        else
        {
            // Seed only essential production data (categories)
            logger.LogInformation("Production environment detected - seeding essential data only");
            await seeder.SeedProductionAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ISK Express API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "ISK Express API Documentation";
    options.EnableDeepLinking();
    options.EnableFilter();
    options.ShowExtensions();
});

// Enable detailed HTTP logging only in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
}

// Global exception handling middleware
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(contextFeature.Error, "An unhandled exception occurred");

            var response = new
            {
                error = "An internal server error occurred",
                message = app.Environment.IsDevelopment() ? contextFeature.Error.Message : "Please try again later",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    });
});

app.UseHttpsRedirection();

// Use CORS - Allow all for temporary private application
app.UseCors("AllowAll");

// Add health check endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                duration = x.Value.Duration,
                description = x.Value.Description
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Map controllers
app.MapControllers();

// API information endpoint
app.MapGet("/", () => new
{
    name = "ISK Express API",
    version = "v1.0",
    status = "running",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    endpoints = new
    {
        health = "/health",
        swagger = "/swagger",
        api = "/api"
    }
});

app.Run();
