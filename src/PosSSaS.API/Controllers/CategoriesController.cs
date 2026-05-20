using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Categories;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllCategoriesQuery(), ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> CreateAsync([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
        => Ok(await _mediator.Send(cmd, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> UpdateAsync(Guid id, [FromBody] UpdateCategoryCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id) return BadRequest("Route id and body id do not match.");
        return Ok(await _mediator.Send(cmd, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}
