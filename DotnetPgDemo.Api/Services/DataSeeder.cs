using DotnetPgDemo.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DotnetPgDemo.Api.Services;

public static class DataSeeder
{
    public static async Task SeedData(AppDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any() || context.Orders.Any())
            return;

        // Create users
        var users = new List<User>
        {
            new User
            {
                Username = "admin1",
                Email = "admin1@example.com",
                PasswordHash = HashPassword("password123"),
                Role = UserRole.Admin,
                AdminLevel = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Username = "admin2",
                Email = "admin2@example.com",
                PasswordHash = HashPassword("password123"),
                Role = UserRole.Admin,
                AdminLevel = 2,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Username = "admin3",
                Email = "admin3@example.com",
                PasswordHash = HashPassword("password123"),
                Role = UserRole.Admin,
                AdminLevel = 3,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Username = "user1",
                Email = "user1@example.com",
                PasswordHash = HashPassword("password123"),
                Role = UserRole.User,
                AdminLevel = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Username = "user2",
                Email = "user2@example.com",
                PasswordHash = HashPassword("password123"),
                Role = UserRole.User,
                AdminLevel = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Refresh users from database to get their IDs
        users = await context.Users.ToListAsync();

        // Create sample orders
        var orders = new List<Order>
        {
            new Order
            {
                OrderNumber = "ORD-001",
                CreatedByUserId = users[3].Id, // user1
                Amount = 300,
                Description = "Office supplies - pens and paper",
                Status = OrderStatus.Pending,
                ApprovalStatus = ApprovalStatus.AwaitingLevel1,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Order
            {
                OrderNumber = "ORD-002",
                CreatedByUserId = users[4].Id, // user2
                Amount = 1500,
                Description = "Printer cartridges and toner",
                Status = OrderStatus.Pending,
                ApprovalStatus = ApprovalStatus.AwaitingLevel1,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Order
            {
                OrderNumber = "ORD-003",
                CreatedByUserId = users[3].Id, // user1
                Amount = 8000,
                Description = "Computer equipment and monitors",
                Status = OrderStatus.Pending,
                ApprovalStatus = ApprovalStatus.AwaitingLevel1,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Order
            {
                OrderNumber = "ORD-004",
                CreatedByUserId = users[4].Id, // user2
                Amount = 50000,
                Description = "Server and networking equipment",
                Status = OrderStatus.Pending,
                ApprovalStatus = ApprovalStatus.AwaitingLevel1,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Order
            {
                OrderNumber = "ORD-005",
                CreatedByUserId = users[3].Id, // user1
                Amount = 450,
                Description = "Conference room supplies",
                Status = OrderStatus.Pending,
                ApprovalStatus = ApprovalStatus.AwaitingLevel1,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        // Refresh orders from database to get their IDs
        orders = await context.Orders.ToListAsync();

        // Create approval records for each order
        foreach (var order in orders)
        {
            var requiredLevels = GetRequiredApprovalLevels(order.Amount);

            foreach (var level in requiredLevels)
            {
                var approval = new OrderApproval
                {
                    OrderId = order.Id,
                    AdminLevel = level,
                    Decision = ApprovalDecision.Pending,
                    ApprovedByUserId = null,
                    CreatedAt = DateTime.UtcNow
                };
                context.OrderApprovals.Add(approval);
            }
        }

        await context.SaveChangesAsync();

        // Now update approvals with actual data  
        var admin1 = users.First(u => u.Username == "admin1");
        
        // ORD-001 - already approved
        var ord001Order = await context.Orders.Include(o => o.Approvals).FirstOrDefaultAsync(o => o.OrderNumber == "ORD-001");
        if (ord001Order != null)
        {
            var ord001Approval = ord001Order.Approvals.FirstOrDefault(a => a.AdminLevel == 1);
            if (ord001Approval != null)
            {
                ord001Approval.Decision = ApprovalDecision.Approved;
                ord001Approval.ApprovedByUserId = admin1.Id;
                ord001Approval.DecidedAt = DateTime.UtcNow.AddHours(-2);
            }
            ord001Order.Status = OrderStatus.Approved;
            ord001Order.ApprovalStatus = ApprovalStatus.FinallyApproved;
            ord001Order.ApprovedAt = DateTime.UtcNow.AddHours(-2);
        }

        // ORD-002 - Level 1 approved, Level 2 pending
        var ord002Order = await context.Orders.Include(o => o.Approvals).FirstOrDefaultAsync(o => o.OrderNumber == "ORD-002");
        if (ord002Order != null)
        {
            var ord002Approval1 = ord002Order.Approvals.FirstOrDefault(a => a.AdminLevel == 1);
            if (ord002Approval1 != null)
            {
                ord002Approval1.Decision = ApprovalDecision.Approved;
                ord002Approval1.ApprovedByUserId = admin1.Id;
                ord002Approval1.DecidedAt = DateTime.UtcNow.AddHours(-3);
            }
            ord002Order.ApprovalStatus = ApprovalStatus.AwaitingLevel2;
        }

        await context.SaveChangesAsync();
    }

    private static List<int> GetRequiredApprovalLevels(decimal amount)
    {
        var requiredLevels = new List<int>();

        if (amount <= 500m)
        {
            requiredLevels.Add(1);
        }
        else if (amount <= 2000m)
        {
            requiredLevels.AddRange(new[] { 1, 2 });
        }
        else if (amount <= 15000m)
        {
            requiredLevels.AddRange(new[] { 1, 2, 3 });
        }
        else
        {
            requiredLevels.AddRange(new[] { 1, 2, 3 });
        }

        return requiredLevels;
    }

    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
