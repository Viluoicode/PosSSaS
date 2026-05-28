using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;
using PosSSaS.Domain.Enums;

namespace PosSSaS.Application.Features.Tenants.Commands.RegisterTenant;

public record RegisterTenantCommand(
    string TenantName,
    string AdminUsername,
    string AdminPassword,
    string FirstBranchName)
    : IRequest<RegisterTenantResult>;

public record RegisterTenantResult(
    Guid TenantId, string TenantName,
    Guid AdminUserId, string AdminUsername,
    Guid BranchId, string BranchName);

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdminUsername).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstBranchName).NotEmpty().MaximumLength(200);
    }
}

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, RegisterTenantResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;

    public RegisterTenantCommandHandler(IApplicationDbContext db, IPasswordHasher hasher)
    {
        _db = db; _hasher = hasher;
    }

    public async Task<RegisterTenantResult> Handle(RegisterTenantCommand request, CancellationToken ct)
    {
        var exists = await _db.Tenants.AnyAsync(t => t.Name == request.TenantName, ct);
        if (exists)
            throw new InvalidOperationException($"Tenant '{request.TenantName}' already exists.");

        var tenant = new Tenant { Name = request.TenantName, IsActive = true };

        var branch = new Branch
        {
            TenantId = tenant.Id,
            Name = request.FirstBranchName,
            IsActive = true
        };

        // Admin has no branch lock — can re-login to any branch in the tenant later.
        var admin = new User
        {
            TenantId = tenant.Id,
            BranchId = null,
            Username = request.AdminUsername,
            PasswordHash = _hasher.Hash(request.AdminPassword),
            Role = UserRole.Admin
        };

        var defaultCategory = new Category
        {
            TenantId = tenant.Id,
            Name = "General",
            IsActive = true
        };

        _db.Tenants.Add(tenant);
        _db.Branches.Add(branch);
        _db.Users.Add(admin);
        _db.Categories.Add(defaultCategory);
        await _db.SaveChangesAsync(ct);

        return new RegisterTenantResult(
            tenant.Id, tenant.Name,
            admin.Id, admin.Username,
            branch.Id, branch.Name);
    }
}
