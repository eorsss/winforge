using WinForge.Shared.DTOs;

namespace WinForge.Core.Optimization;

/// <summary>
/// 1D Profile Cutting Optimizer using First-Fit Decreasing (FFD) bin-packing.
/// 
/// FFD is the industry-standard heuristic for linear cutting optimization.
/// It typically achieves within 11/9 * OPT + 6/9 of the theoretical minimum.
/// For window/door profiles the real-world results are excellent (96–99% yield).
/// 
/// Algorithm:
///   1. Expand all pieces (e.g., 3× 1200mm → three 1200mm items)
///   2. Sort descending by length (longest first → FFD)
///   3. For each piece, find the first existing bar that fits. If none, open a new bar.
///   4. Return full cut plan with visual position info for rendering.
/// </summary>
public class ProfileCuttingOptimizer
{
    /// <summary>
    /// Runs the First-Fit Decreasing cutting optimization.
    /// </summary>
    public static CuttingOptimizationResult Optimize(CuttingOptimizationRequest request)
    {
        if (request.StockLengthMm <= 0)
            throw new ArgumentException("Stock length must be positive.", nameof(request));
        if (request.KerfMm < 0)
            throw new ArgumentException("Kerf must be >= 0.", nameof(request));
        if (!request.Pieces.Any())
            return EmptyResult(request.StockLengthMm);

        double usableStock = request.StockLengthMm;

        // Step 1: Expand quantities into individual cut items
        var items = request.Pieces
            .SelectMany(p => Enumerable.Range(0, p.Quantity)
                .Select(_ => new CutItem(p.Label, p.LengthMm)))
            .ToList();

        // Validate: no piece longer than stock bar
        var tooLong = items.FirstOrDefault(i => i.LengthMm > usableStock);
        if (tooLong != null)
            throw new InvalidOperationException(
                $"Piece '{tooLong.Label}' ({tooLong.LengthMm}mm) is longer than the stock bar ({usableStock}mm).");

        // Step 2: Sort descending (FFD)
        items.Sort((a, b) => b.LengthMm.CompareTo(a.LengthMm));

        // Step 3: FFD bin packing
        var bars = new List<Bar>();

        foreach (var item in items)
        {
            // Find first bar with enough remaining space (including kerf)
            var bar = bars.FirstOrDefault(b => b.CanFit(item.LengthMm, request.KerfMm, usableStock));

            if (bar == null)
            {
                bar = new Bar(bars.Count + 1, usableStock);
                bars.Add(bar);
            }

            bar.AddCut(item, request.KerfMm);
        }

        // Step 4: Build result
        return BuildResult(bars, usableStock);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static CuttingOptimizationResult EmptyResult(double stockLength) =>
        new(0, 0, 0, 0, 0, []);

    private static CuttingOptimizationResult BuildResult(List<Bar> bars, double stockLength)
    {
        double totalStock = bars.Count * stockLength;
        double totalUsed = bars.Sum(b => b.UsedMm);
        double totalWaste = totalStock - totalUsed;
        double wastePercent = totalStock > 0 ? Math.Round(totalWaste / totalStock * 100, 2) : 0;

        var barPlans = bars.Select(b =>
        {
            double barWaste = stockLength - b.UsedMm;
            double barWastePct = Math.Round(barWaste / stockLength * 100, 2);

            return new BarCutPlan(
                b.Number,
                stockLength,
                Math.Round(b.UsedMm, 2),
                Math.Round(barWaste, 2),
                barWastePct,
                b.Cuts.ToList()
            );
        }).ToList();

        return new CuttingOptimizationResult(
            bars.Count,
            Math.Round(totalStock, 2),
            Math.Round(totalUsed, 2),
            Math.Round(totalWaste, 2),
            Math.Round(wastePercent, 2),
            barPlans
        );
    }

    // ── Internal types ────────────────────────────────────────────────────────

    private record CutItem(string Label, double LengthMm);

    private class Bar(int number, double _stockLengthMm)
    {
        public int Number { get; } = number;

        // Position cursor — tracks where the next cut starts
        private double _cursor = 0;
        private readonly List<PlacedCut> _cuts = [];

        public double UsedMm => _cursor;
        public IReadOnlyList<PlacedCut> Cuts => _cuts;

        public bool CanFit(double lengthMm, double kerfMm, double stockLength)
        {
            // First cut on a bar has no kerf before it; subsequent cuts need a kerf gap
            double kerfNeeded = _cuts.Count > 0 ? kerfMm : 0;
            return _cursor + kerfNeeded + lengthMm <= stockLength;
        }

        public void AddCut(CutItem item, double kerfMm)
        {
            double kerfNeeded = _cuts.Count > 0 ? kerfMm : 0;
            double startPos = _cursor + kerfNeeded;

            _cuts.Add(new PlacedCut(item.Label, item.LengthMm, Math.Round(startPos, 2)));
            _cursor = startPos + item.LengthMm;
        }
    }
}
