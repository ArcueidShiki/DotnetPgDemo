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
}
