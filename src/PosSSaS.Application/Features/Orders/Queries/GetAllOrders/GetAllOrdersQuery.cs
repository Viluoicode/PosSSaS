using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Orders.Dtos;

namespace PosSSaS.Application.Features.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery(DateTime? From = null, DateTime? To = null)
    : IRequest<IReadOnlyList<OrderSummaryDto>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IReadOnlyList<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllOrdersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        var query = _db.Orders.AsNoTracking();
        if (request.From.HasValue) query = query.Where(o => o.OrderDate >= request.From.Value);
        if (request.To.HasValue) query = query.Where(o => o.OrderDate <= request.To.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryDto(o.Id, o.OrderDate, o.TotalAmount, o.Status, o.Items.Count))
            .ToListAsync(ct);
    }
}
