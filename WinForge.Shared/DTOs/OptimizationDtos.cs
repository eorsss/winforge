namespace WinForge.Shared.DTOs;

// ── Optimizer Request ─────────────────────────────────────────────────────────

/// <summary>
/// Request to run the 1D profile cutting optimizer.
/// All lengths are in millimetres.
/// </summary>
public record CuttingOptimizationRequest(
    /// <summary>Length of each stock bar (e.g. 6000 mm)</summary>
    double StockLengthMm,
    /// <summary>Saw kerf / blade thickness loss in mm (e.g. 3.2 mm)</summary>
    double KerfMm,
    /// <summary>List of required cut pieces</summary>
    List<CutPieceRequest> Pieces
);

public record CutPieceRequest(
    string Label,       // e.g. "Top rail", "Sash stile"
    double LengthMm,    // required length
    int Quantity        // how many of this piece
);

// ── Optimizer Response ────────────────────────────────────────────────────────

public record CuttingOptimizationResult(
    int TotalBarsUsed,
    double TotalStockLengthMm,
    double TotalUsedMm,
    double TotalWasteMm,
    double WastePercent,
    List<BarCutPlan> BarPlans
);

public record BarCutPlan(
    int BarNumber,
    double StockLengthMm,
    double UsedMm,
    double WasteMm,
    double WastePercent,
    List<PlacedCut> Cuts
);

public record PlacedCut(
    string Label,
    double LengthMm,
    double StartPositionMm  // for visualization
);
