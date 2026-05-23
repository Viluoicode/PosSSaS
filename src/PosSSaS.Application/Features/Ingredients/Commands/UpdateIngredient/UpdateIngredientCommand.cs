using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Application.Features.Ingredients.Dtos;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Application.Features.Ingredients.Commands.UpdateIngredient;

public record UpdateIngredientCommand(Guid Id, string Name, string Unit) : IRequest<IngredientDto>;

public class UpdateIngredientCommandValidator : AbstractValidator<UpdateIngredientCommand>
{
    public UpdateIngredientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}

public class UpdateIngredientCommandHandler : IRequestHandler<UpdateIngredientCommand, IngredientDto>
{
    private readonly IApplicationDbContext _db;
    public UpdateIngredientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<IngredientDto> Handle(UpdateIngredientCommand request, CancellationToken ct)
    {
        var ing = await _db.Ingredients.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Ingredient), request.Id);

        ing.Name = request.Name;
        ing.Unit = request.Unit;
        await _db.SaveChangesAsync(ct);
        return new IngredientDto(ing.Id, ing.Name, ing.Unit, ing.CreatedAt);
    }
}
