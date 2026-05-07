using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

public class Product : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Recipe> RecipeItems { get; set; } = new List<Recipe>();
}
