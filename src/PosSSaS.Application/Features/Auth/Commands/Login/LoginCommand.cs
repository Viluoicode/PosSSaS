using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Enums;

namespace PosSSaS.Application.Features.Auth.Commands.Login;

public record LoginCommand(string TenantName, string Username, string Password, string BranchName)
    : IRequest<LoginResult>;

public record LoginResult(
    string Token,
    DateTime ExpiresAt,
    string Username,
    string Role,
    Guid TenantId,
    string TenantName,
    Guid BranchId,
    string BranchName);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty();
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public LoginCommandHandler(IApplicationDbContext db, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _db = db; _hasher = hasher; _jwt = jwt;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Name == request.TenantName && t.IsActive, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        // No tenant context yet on the principal — bypass filters and scope manually.
        var branch = await _db.Branches.IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.TenantId == tenant.Id && b.Name == request.BranchName && b.IsActive, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Username == request.Username, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // Cashier is locked to one assigned branch. Admin (BranchId null) can pick any branch.
        if (user.Role == UserRole.Cashier && user.BranchId.HasValue && user.BranchId.Value != branch.Id)
            throw new UnauthorizedAccessException("You are not assigned to this branch.");

        var token = _jwt.GenerateToken(user, branch.Id, out var expiresAt);
        return new LoginResult(
            token, expiresAt,
            user.Username, user.Role.ToString(),
            tenant.Id, tenant.Name,
            branch.Id, branch.Name);
    }
}
