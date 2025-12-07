using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetPgDemo.Api.Models;

public class OrderApproval
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    public int? ApprovedByUserId { get; set; }

    [Required]
    public int AdminLevel { get; set; } // 1, 2, or 3

    [Required]
    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

    [MaxLength(500)]
    public string Comments { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DecidedAt { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    [ForeignKey(nameof(ApprovedByUserId))]
    public User? ApprovedByUser { get; set; }
}

public enum ApprovalDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
