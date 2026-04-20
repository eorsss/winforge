using WinForge.Shared.Enums;

namespace WinForge.Infrastructure.Data.Entities;

// ── Multi-tenant base ─────────────────────────────────────────────────────────

public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// ── Tenant (Company) ──────────────────────────────────────────────────────────

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string DefaultCurrency { get; set; } = "EUR";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AppUser> Users { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}

// ── User ──────────────────────────────────────────────────────────────────────

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Sales;
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}

// ── Customer ──────────────────────────────────────────────────────────────────

public class Customer : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}

// ── Order ─────────────────────────────────────────────────────────────────────

public class Order : TenantEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public string Currency { get; set; } = "EUR";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderLine> Lines { get; set; } = [];
}

// ── OrderLine ─────────────────────────────────────────────────────────────────

public class OrderLine : TenantEntity
{
    public Guid OrderId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public MaterialType Material { get; set; }
    public WindowShape Shape { get; set; }
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public int Quantity { get; set; }
    public string? Color { get; set; }
    public string? GlassType { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public double GlassAreaSqm { get; set; }
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
}
