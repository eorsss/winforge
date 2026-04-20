using WinForge.Shared.DTOs;
using WinForge.Shared.Enums;

namespace WinForge.Core.Pricing;

/// <summary>
/// Calculates prices and physical dimensions for a window/door order line.
/// 
/// Glass area is calculated from the opening dimensions minus the frame depth.
/// Profile perimeter length is the sum of all frame members (4 sides for rectangle).
/// This is the same logic used in all window software — you know this from the factory!
/// </summary>
public static class QuoteCalculator
{
    // Default frame depth deduction per side (mm) — typical PVC outer frame
    private const double DefaultFrameDepthMm = 65.0;

    /// <summary>
    /// Calculates glass area in m² for a given window shape and dimensions.
    /// Deducts frame depth from both width and height.
    /// </summary>
    public static double CalcGlassAreaSqm(
        double widthMm, double heightMm,
        WindowShape shape,
        double frameDepthMm = DefaultFrameDepthMm)
    {
        double glassW = Math.Max(0, widthMm - frameDepthMm * 2);
        double glassH = Math.Max(0, heightMm - frameDepthMm * 2);

        return shape switch
        {
            WindowShape.Rectangle => Math.Round(glassW * glassH / 1_000_000, 4),
            WindowShape.Arch      => CalcArchGlassArea(glassW, glassH),
            _                     => Math.Round(glassW * glassH / 1_000_000, 4) // fallback
        };
    }

    /// <summary>
    /// Calculates total profile running metres needed for the outer frame.
    /// Rectangle = 2×(W + H). Arch adds the arch perimeter.
    /// </summary>
    public static double CalcProfileRunningMetres(
        double widthMm, double heightMm, WindowShape shape)
    {
        double perimMm = shape switch
        {
            WindowShape.Rectangle => 2 * (widthMm + heightMm),
            WindowShape.Arch      => 2 * heightMm + widthMm + Math.PI * (widthMm / 2),
            _                     => 2 * (widthMm + heightMm)
        };
        return Math.Round(perimMm / 1000, 3); // convert to metres
    }

    /// <summary>
    /// Calculates the line total: unitPrice × quantity (+ optional glass surcharge).
    /// </summary>
    public static decimal CalcLineTotal(decimal unitPrice, int quantity)
        => unitPrice * quantity;

    // ── Private ───────────────────────────────────────────────────────────────

    private static double CalcArchGlassArea(double glassW, double glassH)
    {
        // Arch = rectangle body + semicircle top
        double radius = glassW / 2;
        double rectH = Math.Max(0, glassH - radius);
        double rectArea = glassW * rectH;
        double archArea = Math.PI * radius * radius / 2;
        return Math.Round((rectArea + archArea) / 1_000_000, 4);
    }
}
