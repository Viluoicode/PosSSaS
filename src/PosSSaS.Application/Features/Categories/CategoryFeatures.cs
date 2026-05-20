using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Categories;

public record CategoryDto(Guid Id, string Name, string? Description, bool IsActive);

// ---------------- Create ----------------
public record CreateCategoryCommand(string Name, string? Description) : IRequest<CategoryDto>;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public CreateCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(CreateCategoryCommand r, CancellationToken ct)
    {
        var c = new Category { Name = r.Name, Description = r.Description, IsActive = true };
        _db.Categories.Add(c);
        await _db.SaveChangesAsync(ct);
        return new CategoryDto(c.Id, c.Name, c.Description, c.IsActive);
    }
}

// ---------------- Update ----------------
public record UpdateCategoryCommand(Guid Id, string Name, string? Description, bool IsActive) : IRequest<CategoryDto>;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    public UpdateCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(UpdateCategoryCommand r, CancellationToken ct)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new NotFoundException(nameof(Category), r.Id);
        c.Name = r.Name; c.Description = r.Description; c.IsActive = r.IsActive;
        await _db.SaveChangesAsync(ct);
        return new CategoryDto(c.Id, c.Name, c.Description, c.IsActive);
    }
}

// ---------------- Delete ----------------
public record DeleteCategoryCommand(Guid Id) : IRequest;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteCategoryCommand r, CancellationToken ct)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new NotFoundException(nameof(Category), r.Id);

        if (await _db.Products.AnyAsync(p => p.CategoryId == r.Id, ct))
            throw new InvalidOperationException(
                $"Category '{c.Name}' is referenced by one or more products and cannot be deleted.");

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync(ct);
    }
}

// ---------------- Queries ----------------
public record GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllCategoriesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetAllCategoriesQuery r, CancellationToken ct)
        => await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IsActive))
            .ToListAsync(ct);
}
