using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Recipes.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Recipes.Commands.SetRecipe;

public record SetRecipeLineDto(Guid IngredientId, decimal QuantityRequired);

/// <summary>Replaces the entire BOM of a product with the supplied lines (idempotent set-operation).</summary>
public record SetRecipeCommand(Guid ProductId, IReadOnlyList<SetRecipeLineDto> Lines)
    : IRequest<ProductRecipeDto>;

public class SetRecipeCommandValidator : AbstractValidator<SetRecipeCommand>
{
    public SetRecipeCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Lines).NotNull();
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(i => i.IngredientId).NotEmpty();
            l.RuleFor(i => i.QuantityRequired).GreaterThan(0);
        });
    }
}

public class SetRecipeCommandHandler : IRequestHandler<SetRecipeCommand, ProductRecipeDto>
{
    private readonly IApplicationDbContext _db;
    public SetRecipeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductRecipeDto> Handle(SetRecipeCommand request, CancellationToken ct)
    {
        var product = await _db.Products
            .Include(p => p.RecipeItems)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // Dedup by IngredientId, sum quantities if caller sent duplicates.
        var requested = request.Lines
            .GroupBy(l => l.IngredientId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantityRequired));

        // Verify every referenced ingredient exists in this tenant (global filter handles cross-tenant).
        var ingredientIds = requested.Keys.ToList();
        var ingredients = await _db.Ingredients
            .Where(i => ingredientIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, ct);

        foreach (var id in ingredientIds)
            if (!ingredients.ContainsKey(id))
                throw new NotFoundException(nameof(Ingredient), id);

        // Replace strategy: drop existing lines and re-add. Simple and correct for small BOMs.
        _db.Recipes.RemoveRange(product.RecipeItems);

        foreach (var (ingId, qty) in requested)
        {
            _db.Recipes.Add(new Recipe
            {
                ProductId = product.Id,
                IngredientId = ingId,
                QuantityRequired = qty
            });
        }

        await _db.SaveChangesAsync(ct);

        var lines = requested
            .Select(kv => new RecipeLineDto(kv.Key, ingredients[kv.Key].Name, ingredients[kv.Key].Unit, kv.Value))
            .ToList();

        return new ProductRecipeDto(product.Id, product.Name, lines);
    }
}
