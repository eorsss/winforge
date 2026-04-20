using System.ComponentModel.DataAnnotations;

namespace WinForge.Shared.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    int TotalOrders,
    decimal TotalRevenue,
    DateTime CreatedAt
);

public record CreateCustomerRequest(
    [Required, MinLength(2)] string Name,
    string? ContactPerson,
    [EmailAddress] string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country
);

public record CustomerSummaryDto(Guid Id, string Name, string? Country, int TotalOrders);
