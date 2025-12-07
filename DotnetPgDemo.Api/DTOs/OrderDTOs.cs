using DotnetPgDemo.Api.Models;

namespace DotnetPgDemo.Api.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? AdminLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public int? AdminLevel { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string ApprovalStatusMessage { get; set; } = string.Empty;
    public List<OrderApprovalDto> Approvals { get; set; } = new List<OrderApprovalDto>();
    public List<int> RequiredApprovalLevels { get; set; } = new List<int>();
}

public class CreateOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class OrderApprovalDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ApprovedByUserId { get; set; }
    public string ApprovedByUsername { get; set; } = string.Empty;
    public int AdminLevel { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}

public class ApproveOrderDto
{
    public int OrderId { get; set; }
    public int ApprovedByUserId { get; set; }
    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Approved;
    public string Comments { get; set; } = string.Empty;
}

public class ApprovalStatusDto
{
    public int OrderId { get; set; }
    public ApprovalStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<int> RequiredApprovalLevels { get; set; } = new List<int>();
    public List<int> ApprovedLevels { get; set; } = new List<int>();
    public List<int> PendingLevels { get; set; } = new List<int>();
}
