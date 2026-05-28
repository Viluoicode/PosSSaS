namespace PosSSaS.Domain.Common.Interfaces;

/// <summary>
/// Stronger scoping than IMustHaveTenant: the entity belongs to one specific branch
/// within a tenant. Used for per-branch stock and per-branch orders.
/// </summary>
public interface IMustHaveBranch : IMustHaveTenant
{
    Guid BranchId { get; set; }
}
