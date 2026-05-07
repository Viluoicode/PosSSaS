using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;
using PosSSaS.Domain.Enums;

namespace PosSSaS.Domain.Entities;

public class User : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    /// <summary>Null = global within tenant (Admin can pick any branch at login).
    /// Set = Cashier locked to one specific branch.</summary>
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Cashier;
}
