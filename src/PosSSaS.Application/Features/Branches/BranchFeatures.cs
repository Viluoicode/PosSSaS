using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Branches;

public record BranchDto(Guid Id, string Name, string? Address, string? PhoneNumber, bool IsActive);

// ---------------- Create ----------------
public record CreateBranchCommand(string Name, string? Address, string? PhoneNumber) : IRequest<BranchDto>;

public class CreateBranchValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, BranchDto>
{
    private readonly IApplicationDbContext _db;
    public CreateBranchHandler(IApplicationDbContext db) => _db = db;

    public async Task<BranchDto> Handle(CreateBranchCommand r, CancellationToken ct)
    {
        var br = new Branch { Name = r.Name, Address = r.Address, PhoneNumber = r.PhoneNumber, IsActive = true };
        _db.Branches.Add(br);

        // Auto-create zero-stock rows for every existing ingredient so the new branch
        // can start receiving stock adjustments without manual setup.
        var ingredients = await _db.Ingredients.AsNoTracking().Select(i => i.Id).ToListAsync(ct);
        foreach (var ingId in ingredients)
        {
            _db.IngredientStocks.Add(new IngredientStock
            {
                BranchId = br.Id,
                IngredientId = ingId,
                Quantity = 0
            });
        }

        await _db.SaveChangesAsync(ct);
        return new BranchDto(br.Id, br.Name, br.Address, br.PhoneNumber, br.IsActive);
    }
}

// ---------------- Update ----------------
public record UpdateBranchCommand(Guid Id, string Name, string? Address, string? PhoneNumber, bool IsActive)
    : IRequest<BranchDto>;

public class UpdateBranchValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, BranchDto>
{
    private readonly IApplicationDbContext _db;
    public UpdateBranchHandler(IApplicationDbContext db) => _db = db;

    public async Task<BranchDto> Handle(UpdateBranchCommand r, CancellationToken ct)
    {
        // Branch is tenant-scoped (not branch-scoped) so we use IgnoreQueryFilters for branch
        // - actually no, Branch implements only IMustHaveTenant. Filter is tenant-only. OK.
        var br = await _db.Branches.FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new NotFoundException(nameof(Branch), r.Id);
        br.Name = r.Name; br.Address = r.Address; br.PhoneNumber = r.PhoneNumber; br.IsActive = r.IsActive;
        await _db.SaveChangesAsync(ct);
        return new BranchDto(br.Id, br.Name, br.Address, br.PhoneNumber, br.IsActive);
    }
}

// ---------------- Queries ----------------
public record GetAllBranchesQuery : IRequest<IReadOnlyList<BranchDto>>;

public class GetAllBranchesHandler : IRequestHandler<GetAllBranchesQuery, IReadOnlyList<BranchDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllBranchesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<BranchDto>> Handle(GetAllBranchesQuery r, CancellationToken ct)
        => await _db.Branches.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.PhoneNumber, b.IsActive))
            .ToListAsync(ct);
}
