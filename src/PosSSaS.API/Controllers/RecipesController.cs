using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Recipes.Commands.SetRecipe;
using PosSSaS.Application.Features.Recipes.Dtos;
using PosSSaS.Application.Features.Recipes.Queries.GetRecipeByProduct;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IMediator _mediator;
    public RecipesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<ProductRecipeDto>> GetByProductAsync(Guid productId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetRecipeByProductQuery(productId), ct));

    [HttpPut("product/{productId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductRecipeDto>> SetAsync(
        Guid productId, [FromBody] SetRecipeRequest body, CancellationToken ct)
    {
        var cmd = new SetRecipeCommand(productId, body.Lines);
        return Ok(await _mediator.Send(cmd, ct));
    }

    public record SetRecipeRequest(IReadOnlyList<SetRecipeLineDto> Lines);
}
