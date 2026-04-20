using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using WinForge.Infrastructure.Data;
using WinForge.Infrastructure.Data.Entities;
using WinForge.Shared.DTOs;
using WinForge.Shared.Enums;
using System.Security.Cryptography;

namespace WinForge.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
}

public class AuthService(WinForgeDbContext db, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            throw new InvalidOperationException("Email already registered.");

        // Create tenant (company)
        var tenant = new Tenant
        {
            CompanyName     = request.CompanyName,
            Country         = request.Country,
            DefaultCurrency = "EUR"
        };
        db.Tenants.Add(tenant);

        // Create first admin user
        var user = new AppUser
        {
            TenantId     = tenant.Id,
            Email        = request.Email.ToLower(),
            PasswordHash = HashPassword(request.Password),
            FirstName    = request.CompanyName, // can update profile later
            LastName     = string.Empty,
            Role         = UserRole.Admin
        };

        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var accessToken  = tokenService.GenerateAccessToken(user, tenant.CompanyName);
        return new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddHours(1),
            user.Email, tenant.CompanyName, user.Role.ToString());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && u.IsActive)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await db.SaveChangesAsync();

        var accessToken = tokenService.GenerateAccessToken(user, user.Tenant.CompanyName);
        return new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddHours(1),
            user.Email, user.Tenant.CompanyName, user.Role.ToString());
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var user = await db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow &&
                u.IsActive)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var newRefresh = tokenService.GenerateRefreshToken();
        user.RefreshToken       = newRefresh;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await db.SaveChangesAsync();

        var accessToken = tokenService.GenerateAccessToken(user, user.Tenant.CompanyName);
        return new AuthResponse(accessToken, newRefresh, DateTime.UtcNow.AddHours(1),
            user.Email, user.Tenant.CompanyName, user.Role.ToString());
    }

    // ── Password helpers (PBKDF2) ─────────────────────────────────────────────

    private static string HashPassword(string password)
    {
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        byte[] hash = KeyDerivation.Pbkdf2(password, salt,
            KeyDerivationPrf.HMACSHA256, 100_000, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = KeyDerivation.Pbkdf2(password, salt,
            KeyDerivationPrf.HMACSHA256, 100_000, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
