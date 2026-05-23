using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Ingredients.Dtos;

namespace PosSSaS.Application.Features.Ingredients.Queries.GetAllIngredients;

public record GetAllIngredientsQuery : IRequest<IReadOnlyList<IngredientDto>>;

public class GetAllIngredientsQueryHandler : IRequestHandler<GetAllIngredientsQuery, IReadOnlyList<IngredientDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllIngredientsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<IngredientDto>> Handle(GetAllIngredientsQuery request, CancellationToken ct)
        => await _db.Ingredients.AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new IngredientDto(i.Id, i.Name, i.Unit, i.CreatedAt))
            .ToListAsync(ct);
}
