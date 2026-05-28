using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Common.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Issues a signed JWT with user id, name, role, tenant_id and branch_id claims.</summary>
    string GenerateToken(User user, Guid branchId, out DateTime expiresAt);
}
