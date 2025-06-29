using Microsoft.EntityFrameworkCore;
using iskxpress_api.Models;

namespace iskxpress_api.Data;

public class IskExpressDbContext : DbContext
{
    public IskExpressDbContext(DbContextOptions<IskExpressDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<Stall> Stalls { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<StallSection> StallSections { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<FileRecord> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);

            entity.HasOne(u => u.ProfilePicture)
                .WithMany()
                .HasForeignKey(u => u.ProfilePictureId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Stall entity
        modelBuilder.Entity<Stall>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);

            // Note: Unique constraint on VendorId is handled by migration SQL
            // to avoid conflicts with foreign key index requirements

            entity.HasOne(s => s.Vendor)
                .WithOne(u => u.Stall)
                .HasForeignKey<Stall>(s => s.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Picture)
                .WithMany()
                .HasForeignKey(s => s.PictureId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(c => c.Vendor)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.VendorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StallSection entity
        modelBuilder.Entity<StallSection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(ss => ss.Stall)
                .WithMany(s => s.StallSections)
                .HasForeignKey(ss => ss.StallId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Section)
                .WithMany(ss => ss.Products)
                .HasForeignKey(p => p.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Stall)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.StallId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Picture)
                .WithMany()
                .HasForeignKey(p => p.PictureId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure CartItem entity
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ci => ci.Stall)
                .WithMany(s => s.CartItems)
                .HasForeignKey(ci => ci.StallId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VendorOrderId).HasMaxLength(50);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Stall)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StallId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.DeliveryPartner)
                .WithMany(u => u.DeliveryOrders)
                .HasForeignKey(o => o.DeliveryPartnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FileRecord entity
        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ObjectKey).HasMaxLength(500);
            entity.Property(e => e.ObjectUrl).HasMaxLength(1000);
            entity.Property(e => e.OriginalFileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
            
            entity.HasIndex(e => new { e.Type, e.EntityId }).IsUnique();
        });

        // Configure enum conversions
        modelBuilder.Entity<User>()
            .Property(e => e.AuthProvider)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(e => e.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(e => e.FulfillmentMethod)
            .HasConversion<string>();
    }
} 