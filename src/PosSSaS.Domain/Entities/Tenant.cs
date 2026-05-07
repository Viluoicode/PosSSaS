using PosSSaS.Domain.Common;

namespace PosSSaS.Domain.Entities;

/// <summary>
/// SYSTEM-level entity. Does NOT implement IMustHaveTenant — it IS the tenant.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
}
