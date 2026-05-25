using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Recipes.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Recipes.Queries.GetRecipeByProduct;

public record GetRecipeByProductQuery(Guid ProductId) : IRequest<ProductRecipeDto>;

public class GetRecipeByProductQueryHandler : IRequestHandler<GetRecipeByProductQuery, ProductRecipeDto>
{
    private readonly IApplicationDbContext _db;
    public GetRecipeByProductQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductRecipeDto> Handle(GetRecipeByProductQuery request, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var lines = await _db.Recipes.AsNoTracking()
            .Where(r => r.ProductId == request.ProductId)
            .OrderBy(r => r.Ingredient.Name)
            .Select(r => new RecipeLineDto(r.Ingredient.Id, r.Ingredient.Name, r.Ingredient.Unit, r.QuantityRequired))
            .ToListAsync(ct);

        return new ProductRecipeDto(product.Id, product.Name, lines);
    }
}
