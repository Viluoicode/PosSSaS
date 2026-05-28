namespace PosSSaS.Application.Common.Interfaces;

/// <summary>
/// Scoped accessor for the authenticated principal. Resolved per request from the JWT.
/// Returns null TenantId/UserId/BranchId for unauthenticated calls (login, registration, health checks).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    Guid? BranchId { get; }
    string? Username { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
