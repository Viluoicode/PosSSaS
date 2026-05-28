using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSSaS.Application.Features.Orders.Commands.CreateOrder;
using PosSSaS.Application.Features.Orders.Dtos;
using PosSSaS.Application.Features.Orders.Queries.GetAllOrders;
using PosSSaS.Application.Features.Orders.Queries.GetOrderById;

namespace PosSSaS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetAllAsync(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllOrdersQuery(from, to), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetByIdAsync(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetOrderByIdQuery(id), ct));

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateOrderResult>> CreateAsync(
        [FromBody] CreateOrderCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.OrderId }, result);
    }
}
