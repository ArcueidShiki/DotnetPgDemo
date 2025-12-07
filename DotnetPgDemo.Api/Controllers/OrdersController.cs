using DotnetPgDemo.Api.DTOs;
using DotnetPgDemo.Api.Models;
using DotnetPgDemo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetPgDemo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, IAuthorizationService authorizationService, ILogger<OrdersController> logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with their approval status
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.CreatedByUser)
            .Include(o => o.Approvals)
                .ThenInclude(a => a.ApprovedByUser)
            .ToListAsync();

        var orderDtos = orders.Select(MapOrderToDto).ToList();
        return Ok(orderDtos);
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.CreatedByUser)
            .Include(o => o.Approvals)
                .ThenInclude(a => a.ApprovedByUser)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Ok(MapOrderToDto(order));
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        // Validate user exists
        var user = await _context.Users.FindAsync(createOrderDto.CreatedByUserId);
        if (user == null)
            return BadRequest("User not found");

        var order = new Order
        {
            OrderNumber = createOrderDto.OrderNumber,
            CreatedByUserId = createOrderDto.CreatedByUserId,
            Amount = createOrderDto.Amount,
            Description = createOrderDto.Description,
            Status = OrderStatus.Pending,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Determine initial approval status based on amount
        var requiredLevels = _authorizationService.GetRequiredApprovalLevels(order.Amount);
        order.ApprovalStatus = requiredLevels.First() == 1 ? ApprovalStatus.AwaitingLevel1 : ApprovalStatus.Pending;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create initial approval records for each required level
        foreach (var level in requiredLevels)
        {
            var approval = new OrderApproval
            {
                OrderId = order.Id,
                AdminLevel = level,
                Decision = ApprovalDecision.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.OrderApprovals.Add(approval);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, MapOrderToDto(order));
    }

    /// <summary>
    /// Approve or reject an order
    /// </summary>
    [HttpPost("{orderId}/approve")]
    public async Task<ActionResult<OrderDto>> ApproveOrder(int orderId, ApproveOrderDto approvalDto)
    {
        var order = await _context.Orders
            .Include(o => o.CreatedByUser)
            .Include(o => o.Approvals)
                .ThenInclude(a => a.ApprovedByUser)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound("Order not found");

        var approver = await _context.Users.FindAsync(approvalDto.ApprovedByUserId);
        if (approver == null)
            return BadRequest("Approver user not found");

        // Check if approver can approve this order
        if (!_authorizationService.CanApproveOrder(order, approver))
            return Forbid("User is not authorized to approve this order");

        // Find or create the approval record for this level
        var existingApproval = order.Approvals
            .FirstOrDefault(a => a.AdminLevel == approver.AdminLevel);

        if (existingApproval == null)
            return BadRequest("Invalid approval level for this order");

        // Check if already decided
        if (existingApproval.Decision != ApprovalDecision.Pending)
            return BadRequest($"This approval level has already been decided: {existingApproval.Decision}");

        // Update the approval
        existingApproval.Decision = approvalDto.Decision;
        existingApproval.ApprovedByUserId = approvalDto.ApprovedByUserId;
        existingApproval.Comments = approvalDto.Comments;
        existingApproval.DecidedAt = DateTime.UtcNow;

        // Check if order is rejected
        if (approvalDto.Decision == ApprovalDecision.Rejected)
        {
            order.ApprovalStatus = ApprovalStatus.Rejected;
            order.Status = OrderStatus.Rejected;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return Ok(MapOrderToDto(order));
        }

        // Check if order is fully approved
        if (_authorizationService.IsOrderFullyApproved(order))
        {
            order.ApprovalStatus = ApprovalStatus.FinallyApproved;
            order.Status = OrderStatus.Approved;
            order.ApprovedAt = DateTime.UtcNow;
        }
        else
        {
            // Update to next required level
            var requiredLevels = _authorizationService.GetRequiredApprovalLevels(order.Amount);
            var nextLevel = requiredLevels.FirstOrDefault(l => 
                !order.Approvals.Any(a => a.AdminLevel == l && a.Decision == ApprovalDecision.Approved));

            if (nextLevel > 0)
            {
                order.ApprovalStatus = (ApprovalStatus)Enum.Parse(typeof(ApprovalStatus), $"AwaitingLevel{nextLevel}");
            }
        }

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return Ok(MapOrderToDto(order));
    }

    /// <summary>
    /// Get approval status for an order
    /// </summary>
    [HttpGet("{id}/approval-status")]
    public async Task<ActionResult<ApprovalStatusDto>> GetApprovalStatus(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Approvals)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        var requiredLevels = _authorizationService.GetRequiredApprovalLevels(order.Amount);
        var approvedLevels = order.Approvals
            ?.Where(a => a.Decision == ApprovalDecision.Approved)
            .Select(a => a.AdminLevel)
            .Distinct()
            .ToList() ?? new List<int>();

        var pendingLevels = requiredLevels.Except(approvedLevels).ToList();

        return Ok(new ApprovalStatusDto
        {
            OrderId = order.Id,
            Status = order.ApprovalStatus,
            Message = _authorizationService.GetApprovalStatusMessage(order),
            RequiredApprovalLevels = requiredLevels,
            ApprovedLevels = approvedLevels,
            PendingLevels = pendingLevels
        });
    }

    /// <summary>
    /// Get required approval levels for an order amount
    /// </summary>
    [HttpGet("approval-levels/{amount}")]
    public ActionResult<List<int>> GetApprovalLevels(decimal amount)
    {
        var requiredLevels = _authorizationService.GetRequiredApprovalLevels(amount);
        return Ok(requiredLevels);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, OrderDto orderDto)
    {
        if (id != orderDto.Id)
            return BadRequest();

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        order.Description = orderDto.Description;
        order.UpdatedAt = DateTime.UtcNow;

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete an order (only if pending)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Can only delete pending orders");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private OrderDto MapOrderToDto(Order order)
    {
        var requiredLevels = _authorizationService.GetRequiredApprovalLevels(order.Amount);
        
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CreatedByUserId = order.CreatedByUserId,
            CreatedByUsername = order.CreatedByUser?.Username ?? "Unknown",
            Amount = order.Amount,
            Description = order.Description,
            Status = order.Status,
            ApprovalStatus = order.ApprovalStatus,
            CreatedAt = order.CreatedAt,
            ApprovedAt = order.ApprovedAt,
            ApprovalStatusMessage = _authorizationService.GetApprovalStatusMessage(order),
            Approvals = order.Approvals?.Select(a => new OrderApprovalDto
            {
                Id = a.Id,
                OrderId = a.OrderId,
                ApprovedByUserId = a.ApprovedByUserId ?? 0,
                ApprovedByUsername = a.ApprovedByUser?.Username ?? "Pending",
                AdminLevel = a.AdminLevel,
                Decision = a.Decision,
                Comments = a.Comments,
                CreatedAt = a.CreatedAt,
                DecidedAt = a.DecidedAt
            }).ToList() ?? new List<OrderApprovalDto>(),
            RequiredApprovalLevels = requiredLevels
        };
    }
}
