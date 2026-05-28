using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Orders.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IApplicationDbContext _db;
    public GetOrderByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Order), request.Id);

        var items = order.Items
            .Select(i => new OrderItemDto(i.Id, i.ProductId, i.Product.Name, i.Quantity, i.Price, i.Price * i.Quantity))
            .ToList();

        return new OrderDto(order.Id, order.OrderDate, order.TotalAmount, order.Status, items);
    }
}
