using MediatR;

namespace PosSSaS.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record CreateOrderCommand(IReadOnlyList<CreateOrderItemDto> Items)
    : IRequest<CreateOrderResult>;

public record CreateOrderResult(Guid OrderId, decimal TotalAmount, DateTime OrderDate);
