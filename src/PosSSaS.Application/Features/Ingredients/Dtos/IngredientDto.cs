namespace PosSSaS.Application.Features.Ingredients.Dtos;

/// <summary>Metadata only. Per-branch on-hand quantity lives in the Stock feature.</summary>
public record IngredientDto(Guid Id, string Name, string Unit, DateTime CreatedAt);
