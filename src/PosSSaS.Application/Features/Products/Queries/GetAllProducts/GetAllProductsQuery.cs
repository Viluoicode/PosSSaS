using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Products.Dtos;

namespace PosSSaS.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery(bool? ActiveOnly = null, Guid? CategoryId = null)
    : IRequest<IReadOnlyList<ProductDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllProductsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken ct)
    {
        var query = _db.Products.AsNoTracking();
        if (request.ActiveOnly == true) query = query.Where(p => p.IsActive);
        if (request.CategoryId.HasValue) query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.IsActive, p.CategoryId, p.Category.Name, p.CreatedAt))
            .ToListAsync(ct);
    }
}
