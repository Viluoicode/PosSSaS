using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Ingredients.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Ingredients.Commands.CreateIngredient;

/// <summary>Creates the ingredient metadata AND auto-creates zero-quantity stock rows
/// for every existing branch in the tenant. That way Admin can immediately call
/// AdjustStock on any branch without manual stock-row creation.</summary>
public record CreateIngredientCommand(string Name, string Unit) : IRequest<IngredientDto>;

public class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
{
    public CreateIngredientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, IngredientDto>
{
    private readonly IApplicationDbContext _db;
    public CreateIngredientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<IngredientDto> Handle(CreateIngredientCommand request, CancellationToken ct)
    {
        var ing = new Ingredient { Name = request.Name, Unit = request.Unit };
        _db.Ingredients.Add(ing);

        // Fan out zero stock rows to every branch of this tenant.
        var branchIds = await _db.Branches.AsNoTracking().Select(b => b.Id).ToListAsync(ct);
        foreach (var brId in branchIds)
        {
            _db.IngredientStocks.Add(new IngredientStock
            {
                BranchId = brId,
                IngredientId = ing.Id,
                Quantity = 0
            });
        }

        await _db.SaveChangesAsync(ct);
        return new IngredientDto(ing.Id, ing.Name, ing.Unit, ing.CreatedAt);
    }
}
