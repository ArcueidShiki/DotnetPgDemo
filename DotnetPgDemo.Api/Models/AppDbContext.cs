using System;
using Microsoft.EntityFrameworkCore;

namespace DotnetPgDemo.Api.Models;

public class AppDbContext : DbContext // create, close connection to the database
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    // corresponse to the one table in the database
    public DbSet<Person> People { get; set; }
    
    // Authorization and Order Management
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderApproval> OrderApprovals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>()
            .HasMany(u => u.CreatedOrders)
            .WithOne(o => o.CreatedByUser)
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Approvals)
            .WithOne(oa => oa.ApprovedByUser)
            .HasForeignKey(oa => oa.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Order entity
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Approvals)
            .WithOne(oa => oa.Order)
            .HasForeignKey(oa => oa.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrderApproval entity
        modelBuilder.Entity<OrderApproval>()
            .HasOne(oa => oa.Order)
            .WithMany(o => o.Approvals)
            .HasForeignKey(oa => oa.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderApproval>()
            .HasOne(oa => oa.ApprovedByUser)
            .WithMany(u => u.Approvals)
            .HasForeignKey(oa => oa.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
