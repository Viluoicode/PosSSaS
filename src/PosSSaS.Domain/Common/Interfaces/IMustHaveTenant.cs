namespace PosSSaS.Domain.Common.Interfaces;

/// <summary>
/// Marker contract for every tenant-scoped aggregate. The DbContext uses it to
/// apply a global query filter and to auto-populate TenantId on insert.
/// </summary>
public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}
