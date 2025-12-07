using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetPgDemo.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    // If user is admin, this represents their admin level (1, 2, or 3)
    public int? AdminLevel { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    public ICollection<OrderApproval> Approvals { get; set; } = new List<OrderApproval>();
}

public enum UserRole
{
    User = 0,
    Admin = 1
}
