using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

/// <summary>
/// Tenant-wide metadata for a raw material. Actual on-hand quantity lives in
/// IngredientStock (per-branch). One Ingredient row → many IngredientStock rows.
/// </summary>
public class Ingredient : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    /// <summary>e.g. "gram", "ml", "pcs".</summary>
    public string Unit { get; set; } = string.Empty;

    public ICollection<IngredientStock> Stocks { get; set; } = new List<IngredientStock>();
}
