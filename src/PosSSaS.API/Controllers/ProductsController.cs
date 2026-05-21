using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Products.Commands.CreateProduct;
using PosSSaS.Application.Features.Products.Commands.DeleteProduct;
using PosSSaS.Application.Features.Products.Commands.UpdateProduct;
using PosSSaS.Application.Features.Products.Dtos;
using PosSSaS.Application.Features.Products.Queries.GetAllProducts;
using PosSSaS.Application.Features.Products.Queries.GetProductById;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAllAsync(
        [FromQuery] bool? activeOnly,
        CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllProductsQuery(activeOnly), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetProductByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateAsync([FromBody] CreateProductCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateAsync(Guid id, [FromBody] UpdateProductCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id) return BadRequest("Route id and body id do not match.");
        return Ok(await _mediator.Send(cmd, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }
}
