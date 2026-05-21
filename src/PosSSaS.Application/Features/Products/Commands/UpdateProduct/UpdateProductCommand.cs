using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Products.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, bool IsActive, Guid CategoryId)
    : IRequest<ProductDto>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _db;
    public UpdateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var p = await _db.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        if (p.CategoryId != request.CategoryId)
        {
            var newCat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);
            p.CategoryId = newCat.Id;
            p.Category = newCat;
        }

        p.Name = request.Name;
        p.Price = request.Price;
        p.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return new ProductDto(p.Id, p.Name, p.Price, p.IsActive, p.CategoryId, p.Category.Name, p.CreatedAt);
    }
}
