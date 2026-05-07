using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

public class Branch : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<IngredientStock> Stocks { get; set; } = new List<IngredientStock>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
