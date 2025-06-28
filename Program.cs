using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using iskxpress_api.Data;
using iskxpress_api.Repositories;
using iskxpress_api.Services;
using Amazon.S3;

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

// Add controllers
builder.Services.AddControllers();

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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ISK Express API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "ISK Express API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Map controllers
app.MapControllers();

// Sample endpoint
app.MapGet("/", () => "ISK Express API is running!");

app.Run();
