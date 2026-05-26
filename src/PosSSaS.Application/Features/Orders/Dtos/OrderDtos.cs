using PosSSaS.Domain.Enums;

namespace PosSSaS.Application.Features.Orders.Dtos;

public record OrderItemDto(Guid Id, Guid ProductId, string ProductName, int Quantity, decimal Price, decimal LineTotal);

public record OrderDto(
    Guid Id,
    DateTime OrderDate,
    decimal TotalAmount,
    OrderStatus Status,
    IReadOnlyList<OrderItemDto> Items);

public record OrderSummaryDto(Guid Id, DateTime OrderDate, decimal TotalAmount, OrderStatus Status, int ItemCount);
