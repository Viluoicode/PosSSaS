namespace PosSSaS.Application.Features.Recipes.Dtos;

public record RecipeLineDto(Guid IngredientId, string IngredientName, string Unit, decimal QuantityRequired);

public record ProductRecipeDto(Guid ProductId, string ProductName, IReadOnlyList<RecipeLineDto> Lines);
