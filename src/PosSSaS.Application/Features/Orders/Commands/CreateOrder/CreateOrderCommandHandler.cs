using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;
using PosSSaS.Domain.Enums;

namespace PosSSaS.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Branch-aware checkout. Steps:
///   1) Load products + recipes (tenant-filtered automatically).
///   2) Roll up ingredient demand across all line items.
///   3) Load IngredientStock rows for THIS branch only (branch filter).
///   4) Verify availability; throw before touching anything.
///   5) Persist the order, decrement per-branch stock, wrap in a transaction.
/// RowVersion on IngredientStock surfaces lost-update conflicts as 409.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (!_currentUser.TenantId.HasValue || !_currentUser.BranchId.HasValue)
            throw new UnauthorizedAccessException("Caller has no tenant/branch context.");

        var requested = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var productIds = requested.Keys.ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.RecipeItems)
            .ToListAsync(ct);

        if (products.Count != productIds.Count)
        {
            var missing = productIds.Except(products.Select(p => p.Id)).First();
            throw new NotFoundException(nameof(Product), missing);
        }

        // ingredientId -> total qty needed in this whole order
        var ingredientDemand = new Dictionary<Guid, decimal>();
        foreach (var product in products)
        {
            var qtyOrdered = requested[product.Id];
            foreach (var recipe in product.RecipeItems)
            {
                ingredientDemand.TryGetValue(recipe.IngredientId, out var soFar);
                ingredientDemand[recipe.IngredientId] = soFar + recipe.QuantityRequired * qtyOrdered;
            }
        }

        // Load THIS branch's stock rows for the ingredients we need (branch filter auto-applied).
        var ingredientIds = ingredientDemand.Keys.ToList();
        var stocks = await _db.IngredientStocks
            .Include(s => s.Ingredient)
            .Where(s => ingredientIds.Contains(s.IngredientId))
            .ToDictionaryAsync(s => s.IngredientId, ct);

        foreach (var (ingId, required) in ingredientDemand)
        {
            if (!stocks.TryGetValue(ingId, out var stock))
                // No stock row at this branch — treat as zero on hand.
                throw new InsufficientStockException($"ingredient {ingId}", required, 0);

            if (stock.Quantity < required)
                throw new InsufficientStockException(stock.Ingredient.Name, required, stock.Quantity);
        }

        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Completed,
            Items = products.Select(p => new OrderItem
            {
                ProductId = p.Id,
                Quantity = requested[p.Id],
                Price = p.Price
            }).ToList()
        };
        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

        await using var tx = await _db.BeginTransactionAsync(ct);
        try
        {
            _db.Orders.Add(order);

            foreach (var (ingId, required) in ingredientDemand)
                stocks[ingId].Quantity -= required;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        return new CreateOrderResult(order.Id, order.TotalAmount, order.OrderDate);
    }
}
