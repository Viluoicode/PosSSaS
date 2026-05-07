using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

public class Category : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
