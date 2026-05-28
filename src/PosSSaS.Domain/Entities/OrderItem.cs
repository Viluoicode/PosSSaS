using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;

namespace PosSSaS.Domain.Entities;

public class OrderItem : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    /// <summary>Snapshot of Product.Price at the moment the order was created.</summary>
    public decimal Price { get; set; }

    public decimal LineTotal => Price * Quantity;
}
