using System.ComponentModel.DataAnnotations;

namespace WinForge.Shared.DTOs;

public record RegisterRequest(
    [Required] string CompanyName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    string? PhoneNumber,
    string? Country
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserEmail,
    string CompanyName,
    string Role
);

public record RefreshTokenRequest(string RefreshToken);

/*
## 🏗️ Project Scaffold
- [x] Create solution & projects (WinForge.sln)
- [x] Add project references & NuGet packages (EF Core 9, MudBlazor, QuestPDF, JWT, Swashbuckle)
- [ ] Configure GitHub Actions CI pipeline

## 🔐 Authentication
- [x] JWT token service (access + refresh)
- [x] AuthService (register creates Tenant+Admin, PBKDF2 password hashing)
- [x] AuthController (register, login, refresh endpoints)
- [ ] Blazor login/register pages (UI)
- [x] Role-based access (Admin, Sales, Production, Viewer enums)
- [ ] Invite user by email

## 🗄️ Database & Infrastructure
- [x] EF Core entities (Tenant, AppUser, Customer, Order, OrderLine)
- [x] WinForgeDbContext with schema + indexes
- [ ] Supabase PostgreSQL setup (needs your account)
- [ ] Initial migration (`dotnet ef migrations add InitialCreate`)
- [ ] Multi-tenant middleware (TenantId from JWT claims)
*/
