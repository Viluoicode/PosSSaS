using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;
using PosSSaS.Domain.Enums;

namespace PosSSaS.Domain.Entities;

public class Order : BaseEntity, IMustHaveBranch
{
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
