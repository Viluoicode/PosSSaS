using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var p = await _db.Products
            .Include(x => x.RecipeItems)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        // Cascade removes the recipe lines along with the product.
        _db.Products.Remove(p);
        await _db.SaveChangesAsync(ct);
    }
}
