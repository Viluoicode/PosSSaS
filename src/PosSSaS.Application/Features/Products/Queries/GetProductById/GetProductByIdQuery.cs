using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Products.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IApplicationDbContext _db;
    public GetProductByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        return new ProductDto(p.Id, p.Name, p.Price, p.IsActive, p.CategoryId, p.Category.Name, p.CreatedAt);
    }
}
