using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

/// <summary>
/// Bill-of-Material line: one Product consumes Quantity of one Ingredient.
/// Composite uniqueness on (ProductId, IngredientId) is enforced in Fluent config.
/// </summary>
public class Recipe : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    /// <summary>Amount of Ingredient required per 1 unit of Product (expressed in Ingredient.Unit).</summary>
    public decimal QuantityRequired { get; set; }
}
