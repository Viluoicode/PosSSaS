using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

/// <summary>
/// Per-branch on-hand quantity of an ingredient. The same Ingredient (metadata: name, unit)
/// has one IngredientStock row per Branch. Orders decrement the stock row of the order's branch.
/// </summary>
public class IngredientStock : BaseEntity, IMustHaveBranch
{
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public decimal Quantity { get; set; }

    /// <summary>Concurrency token — prevents two simultaneous orders from overselling.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
