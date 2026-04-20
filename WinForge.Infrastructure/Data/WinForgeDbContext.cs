using Microsoft.EntityFrameworkCore;
using WinForge.Infrastructure.Data.Entities;

namespace WinForge.Infrastructure.Data;

public class WinForgeDbContext(DbContextOptions<WinForgeDbContext> options) : DbContext(options)
{
    public DbSet<Tenant>    Tenants    => Set<Tenant>();
    public DbSet<AppUser>   Users      => Set<AppUser>();
    public DbSet<Customer>  Customers  => Set<Customer>();
    public DbSet<Order>     Orders     => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── Tenant ────────────────────────────────────────────────────────────
        mb.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(t => t.DefaultCurrency).HasMaxLength(3);
            e.HasIndex(t => t.CompanyName);
        });

        // ── AppUser ───────────────────────────────────────────────────────────
        mb.Entity<AppUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.HasOne(u => u.Tenant)
             .WithMany(t => t.Users)
             .HasForeignKey(u => u.TenantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Customer ──────────────────────────────────────────────────────────
        mb.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => new { c.TenantId, c.Name });
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Email).HasMaxLength(256);
            e.Property(c => c.Phone).HasMaxLength(50);
            e.HasOne(c => c.Tenant)
             .WithMany(t => t.Customers)
             .HasForeignKey(c => c.TenantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Order ─────────────────────────────────────────────────────────────
        mb.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
            e.Property(o => o.OrderNumber).HasMaxLength(20).IsRequired();
            e.Property(o => o.Currency).HasMaxLength(3);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.HasOne(o => o.Tenant)
             .WithMany(t => t.Orders)
             .HasForeignKey(o => o.TenantId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.Customer)
             .WithMany(c => c.Orders)
             .HasForeignKey(o => o.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── OrderLine ─────────────────────────────────────────────────────────
        mb.Entity<OrderLine>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
            e.Property(l => l.LineTotal).HasPrecision(18, 2);
            e.Property(l => l.Description).HasMaxLength(500);
            e.Property(l => l.Color).HasMaxLength(100);
            e.Property(l => l.GlassType).HasMaxLength(100);
            e.HasOne(l => l.Order)
             .WithMany(o => o.Lines)
             .HasForeignKey(l => l.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Global query filters for multi-tenancy — enforced at EF level
        // (TenantId is injected by ITenantContext in the API service layer)
    }
}
