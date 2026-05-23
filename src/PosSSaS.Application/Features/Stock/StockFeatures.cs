using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Stock;

public record StockDto(Guid IngredientId, string IngredientName, string Unit, decimal Quantity);

// ---------------- Query: list all stock at the current branch ----------------
public record GetStockListQuery : IRequest<IReadOnlyList<StockDto>>;

public class GetStockListHandler : IRequestHandler<GetStockListQuery, IReadOnlyList<StockDto>>
{
    private readonly IApplicationDbContext _db;
    public GetStockListHandler(IApplicationDbContext db) => _db = db;

    // The branch filter on IngredientStock kicks in automatically — caller sees only their branch.
    // Use navigation property (s.Ingredient) so EF generates a single JOIN; OrderBy on a projected
    // DTO field cannot be translated to SQL, but OrderBy on a nav property can.
    public async Task<IReadOnlyList<StockDto>> Handle(GetStockListQuery r, CancellationToken ct)
        => await _db.IngredientStocks.AsNoTracking()
            .OrderBy(s => s.Ingredient.Name)
            .Select(s => new StockDto(s.Ingredient.Id, s.Ingredient.Name, s.Ingredient.Unit, s.Quantity))
            .ToListAsync(ct);
}

// ---------------- Command: adjust stock (delta) for current branch ----------------
public record AdjustStockCommand(Guid IngredientId, decimal Delta, string? Reason) : IRequest<StockDto>;

public class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.IngredientId).NotEmpty();
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Delta must be non-zero.");
    }
}

public class AdjustStockHandler : IRequestHandler<AdjustStockCommand, StockDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AdjustStockHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db; _currentUser = currentUser;
    }

    public async Task<StockDto> Handle(AdjustStockCommand r, CancellationToken ct)
    {
        if (!_currentUser.BranchId.HasValue)
            throw new UnauthorizedAccessException("No branch context.");

        var stock = await _db.IngredientStocks
            .FirstOrDefaultAsync(s => s.IngredientId == r.IngredientId, ct);

        var ingredient = await _db.Ingredients.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == r.IngredientId, ct)
            ?? throw new NotFoundException(nameof(Ingredient), r.IngredientId);

        // Defensive lazy-create: if the ingredient was created before this branch existed and
        // we somehow missed the fan-out (e.g. data imported via SQL), make a fresh stock row.
        if (stock is null)
        {
            stock = new IngredientStock { IngredientId = r.IngredientId, Quantity = 0 };
            _db.IngredientStocks.Add(stock);
        }

        var newQty = stock.Quantity + r.Delta;
        if (newQty < 0)
            throw new InsufficientStockException(ingredient.Name, Math.Abs(r.Delta), stock.Quantity);

        stock.Quantity = newQty;
        await _db.SaveChangesAsync(ct);

        return new StockDto(ingredient.Id, ingredient.Name, ingredient.Unit, stock.Quantity);
    }
}
