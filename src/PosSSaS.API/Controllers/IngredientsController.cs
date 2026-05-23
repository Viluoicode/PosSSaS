using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Ingredients.Commands.CreateIngredient;
using PosSSaS.Application.Features.Ingredients.Commands.DeleteIngredient;
using PosSSaS.Application.Features.Ingredients.Commands.UpdateIngredient;
using PosSSaS.Application.Features.Ingredients.Dtos;
using PosSSaS.Application.Features.Ingredients.Queries.GetAllIngredients;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IngredientsController : ControllerBase
{
    private readonly IMediator _mediator;
    public IngredientsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngredientDto>>> GetAllAsync(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllIngredientsQuery(), ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IngredientDto>> CreateAsync([FromBody] CreateIngredientCommand cmd, CancellationToken ct)
        => Ok(await _mediator.Send(cmd, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IngredientDto>> UpdateAsync(Guid id, [FromBody] UpdateIngredientCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id) return BadRequest("Route id and body id do not match.");
        return Ok(await _mediator.Send(cmd, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteIngredientCommand(id), ct);
        return NoContent();
    }
}
