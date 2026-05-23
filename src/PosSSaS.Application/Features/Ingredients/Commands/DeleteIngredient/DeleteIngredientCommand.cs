using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Ingredients.Commands.DeleteIngredient;

public record DeleteIngredientCommand(Guid Id) : IRequest;

public class DeleteIngredientCommandHandler : IRequestHandler<DeleteIngredientCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteIngredientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteIngredientCommand request, CancellationToken ct)
    {
        var ing = await _db.Ingredients
            .Include(i => i.Stocks)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Ingredient), request.Id);

        if (await _db.Recipes.IgnoreQueryFilters()
                .AnyAsync(r => r.IngredientId == request.Id && r.TenantId == ing.TenantId, ct))
            throw new InvalidOperationException(
                $"Ingredient '{ing.Name}' is used in one or more recipes and cannot be deleted.");

        // Remove all per-branch stock rows together with the ingredient metadata.
        _db.IngredientStocks.RemoveRange(ing.Stocks);
        _db.Ingredients.Remove(ing);
        await _db.SaveChangesAsync(ct);
    }
}
