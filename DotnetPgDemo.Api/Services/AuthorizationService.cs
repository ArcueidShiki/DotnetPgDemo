using DotnetPgDemo.Api.Models;

namespace DotnetPgDemo.Api.Services;

public interface IAuthorizationService
{
    /// <summary>
    /// Determines which admin levels are required to approve an order based on its amount
    /// </summary>
    List<int> GetRequiredApprovalLevels(decimal amount);

    /// <summary>
    /// Checks if an order can be approved by a specific admin level
    /// </summary>
    bool CanApproveOrder(Order order, User approver);

    /// <summary>
    /// Gets the approval chain for an order
    /// </summary>
    List<int> GetApprovalChain(Order order);

    /// <summary>
    /// Determines if an order is fully approved
    /// </summary>
    bool IsOrderFullyApproved(Order order);

    /// <summary>
    /// Gets the current approval status message
    /// </summary>
    string GetApprovalStatusMessage(Order order);
}

public class AuthorizationService : IAuthorizationService
{
    // Approval thresholds for each admin level
    // Level 1: up to 500
    // Level 2: up to 2000
    // Level 3: up to 15000
    private const decimal Level1Threshold = 500m;
    private const decimal Level2Threshold = 2000m;
    private const decimal Level3Threshold = 15000m;

    public List<int> GetRequiredApprovalLevels(decimal amount)
    {
        var requiredLevels = new List<int>();

        if (amount <= Level1Threshold)
        {
            requiredLevels.Add(1); // Level 1 can approve
        }
        else if (amount <= Level2Threshold)
        {
            requiredLevels.Add(1);
            requiredLevels.Add(2); // Level 2 can approve, but Level 1 approval may be needed
        }
        else if (amount <= Level3Threshold)
        {
            requiredLevels.Add(1);
            requiredLevels.Add(2);
            requiredLevels.Add(3); // Level 3 can approve, and all levels required
        }
        else
        {
            // Amount exceeds all thresholds - require all three levels
            requiredLevels.Add(1);
            requiredLevels.Add(2);
            requiredLevels.Add(3);
        }

        return requiredLevels;
    }

    public bool CanApproveOrder(Order order, User approver)
    {
        // Only admins can approve orders
        if (approver.Role != UserRole.Admin || !approver.AdminLevel.HasValue)
            return false;

        var adminLevel = approver.AdminLevel.Value;
        var requiredLevels = GetRequiredApprovalLevels(order.Amount);

        // Admin can only approve if their level is in the required levels
        return requiredLevels.Contains(adminLevel);
    }

    public List<int> GetApprovalChain(Order order)
    {
        var chain = new List<int>();
        var requiredLevels = GetRequiredApprovalLevels(order.Amount);

        // Build the approval chain based on order amount
        if (order.Amount <= Level1Threshold)
        {
            chain.Add(1);
        }
        else if (order.Amount <= Level2Threshold)
        {
            chain.AddRange(new[] { 1, 2 });
        }
        else if (order.Amount <= Level3Threshold)
        {
            chain.AddRange(new[] { 1, 2, 3 });
        }
        else
        {
            // Amount exceeds all thresholds
            chain.AddRange(new[] { 1, 2, 3 });
        }

        return chain;
    }

    public bool IsOrderFullyApproved(Order order)
    {
        if (order.ApprovalStatus == ApprovalStatus.FinallyApproved)
            return true;

        var requiredLevels = GetRequiredApprovalLevels(order.Amount);

        // Check if all required levels have approved
        if (order.Approvals == null || !order.Approvals.Any())
            return false;

        var approvedLevels = order.Approvals
            .Where(a => a.Decision == ApprovalDecision.Approved)
            .Select(a => a.AdminLevel)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        // All required levels must have approved
        return requiredLevels.SequenceEqual(approvedLevels);
    }

    public string GetApprovalStatusMessage(Order order)
    {
        if (order.ApprovalStatus == ApprovalStatus.FinallyApproved)
            return "Order is fully approved";

        if (order.ApprovalStatus == ApprovalStatus.Rejected)
            return "Order has been rejected";

        var requiredLevels = GetRequiredApprovalLevels(order.Amount);
        var approvedLevels = order.Approvals
            ?.Where(a => a.Decision == ApprovalDecision.Approved)
            .Select(a => a.AdminLevel)
            .Distinct()
            .OrderBy(x => x)
            .ToList() ?? new List<int>();

        var pendingLevels = requiredLevels.Except(approvedLevels).ToList();

        if (pendingLevels.Count > 0)
        {
            var pendingLevelsList = string.Join(", ", pendingLevels.Select(l => $"Level {l}"));
            return $"Awaiting approval from: {pendingLevelsList}";
        }

        return "Pending approval";
    }
}
