using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetPgDemo.Api.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int CreatedByUserId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedByUser { get; set; }

    // Navigation properties
    public ICollection<OrderApproval> Approvals { get; set; } = new List<OrderApproval>();
}

public enum OrderStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum ApprovalStatus
{
    Pending = 0,
    AwaitingLevel1 = 1,
    AwaitingLevel2 = 2,
    AwaitingLevel3 = 3,
    FinallyApproved = 4,
    Rejected = 5
}
