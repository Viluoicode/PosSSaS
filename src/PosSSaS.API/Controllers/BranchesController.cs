using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Branches;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;
    public BranchesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetAllAsync(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllBranchesQuery(), ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BranchDto>> CreateAsync([FromBody] CreateBranchCommand cmd, CancellationToken ct)
        => Ok(await _mediator.Send(cmd, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BranchDto>> UpdateAsync(Guid id, [FromBody] UpdateBranchCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id) return BadRequest("Route id and body id do not match.");
        return Ok(await _mediator.Send(cmd, ct));
    }
}
