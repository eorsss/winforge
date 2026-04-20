using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WinForge.Core.Optimization;
using WinForge.Shared.DTOs;

namespace WinForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OptimizationController : ControllerBase
{
    /// <summary>
    /// Run the 1D profile cutting optimizer.
    /// Returns an optimized cut plan minimizing material waste.
    /// </summary>
    [HttpPost("profile-cutting")]
    public ActionResult<CuttingOptimizationResult> OptimizeProfileCutting(
        [FromBody] CuttingOptimizationRequest request)
    {
        try
        {
            var result = ProfileCuttingOptimizer.Optimize(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }
}
