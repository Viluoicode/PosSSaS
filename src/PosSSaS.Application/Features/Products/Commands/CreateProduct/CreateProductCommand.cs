using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Products.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(string Name, decimal Price, Guid CategoryId) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _db;
    public CreateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var p = new Product
        {
            Name = request.Name,
            Price = request.Price,
            CategoryId = request.CategoryId,
            IsActive = true
        };
        _db.Products.Add(p);
        await _db.SaveChangesAsync(ct);
        return new ProductDto(p.Id, p.Name, p.Price, p.IsActive, category.Id, category.Name, p.CreatedAt);
    }
}
