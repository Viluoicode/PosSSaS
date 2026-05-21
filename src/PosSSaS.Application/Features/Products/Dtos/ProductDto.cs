namespace PosSSaS.Application.Features.Products.Dtos;

public record ProductDto(Guid Id, string Name, decimal Price, bool IsActive, Guid CategoryId, string CategoryName, DateTime CreatedAt);
