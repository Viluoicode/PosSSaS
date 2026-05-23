using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Stock;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IMediator _mediator;
    public StockController(IMediator mediator) => _mediator = mediator;

    /// <summary>Stock levels at the caller's current branch (from JWT branch_id).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StockDto>>> GetAllAsync(CancellationToken ct)
        => Ok(await _mediator.Send(new GetStockListQuery(), ct));

    [HttpPost("{ingredientId:guid}/adjust")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StockDto>> AdjustAsync(
        Guid ingredientId, [FromBody] AdjustStockRequest body, CancellationToken ct)
        => Ok(await _mediator.Send(new AdjustStockCommand(ingredientId, body.Delta, body.Reason), ct));

    public record AdjustStockRequest(decimal Delta, string? Reason);
}
