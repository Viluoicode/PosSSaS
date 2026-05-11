using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PosSSaS.Application.Common.Interfaces;

namespace PosSSaS.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public const string TenantIdClaim = "tenant_id";
    public const string BranchIdClaim = "branch_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId => TryParseGuid(Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    public Guid? TenantId => TryParseGuid(Principal?.FindFirst(TenantIdClaim)?.Value);
    public Guid? BranchId => TryParseGuid(Principal?.FindFirst(BranchIdClaim)?.Value);
    public string? Username => Principal?.FindFirst(ClaimTypes.Name)?.Value;
    public string? Role => Principal?.FindFirst(ClaimTypes.Role)?.Value;

    private static Guid? TryParseGuid(string? raw)
        => Guid.TryParse(raw, out var g) ? g : null;
}
