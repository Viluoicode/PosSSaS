using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Tenants.Commands.RegisterTenant;

namespace PosSSaS.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterTenantResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterTenantResult>> RegisterAsync(
        [FromBody] RegisterTenantCommand cmd,
        CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(RegisterAsync), new { id = result.TenantId }, result);
    }
}
