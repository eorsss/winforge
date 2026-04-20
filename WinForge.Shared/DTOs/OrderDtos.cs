using System.ComponentModel.DataAnnotations;
using WinForge.Shared.Enums;

namespace WinForge.Shared.DTOs;

// ── Order ────────────────────────────────────────────────────────────────────

public record CreateOrderRequest(
    [Required] Guid CustomerId,
    string? Notes,
    string Currency = "EUR",
    List<CreateOrderLineRequest>? Lines = null
);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    OrderStatus Status,
    string Currency,
    decimal TotalAmount,
    string? Notes,
    DateTime CreatedAt,
    List<OrderLineDto> Lines
);

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    OrderStatus Status,
    decimal TotalAmount,
    string Currency,
    int LineCount,
    DateTime CreatedAt
);

// ── Order Line ────────────────────────────────────────────────────────────────

public record CreateOrderLineRequest(
    [Required] string Description,
    MaterialType Material,
    WindowShape Shape,
    [Range(100, 10000)] double WidthMm,
    [Range(100, 10000)] double HeightMm,
    [Range(1, 9999)] int Quantity,
    string? Color,
    string? GlassType,
    decimal UnitPrice,
    string? Notes
);

public record OrderLineDto(
    Guid Id,
    int LineNumber,
    string Description,
    MaterialType Material,
    WindowShape Shape,
    double WidthMm,
    double HeightMm,
    int Quantity,
    string? Color,
    string? GlassType,
    decimal UnitPrice,
    decimal LineTotal,
    double GlassAreaSqm,
    string? Notes
);
