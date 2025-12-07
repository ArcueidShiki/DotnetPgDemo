using DotnetPgDemo.Api.DTOs;
using DotnetPgDemo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DotnetPgDemo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        var userDtos = users.Select(MapUserToDto).ToList();
        return Ok(userDtos);
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        return Ok(MapUserToDto(user));
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            return BadRequest("Username already exists");

        // Validate admin level if role is admin
        if (createUserDto.Role == UserRole.Admin)
        {
            if (!createUserDto.AdminLevel.HasValue || createUserDto.AdminLevel < 1 || createUserDto.AdminLevel > 3)
                return BadRequest("Admin level must be 1, 2, or 3");
        }

        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            Role = createUserDto.Role,
            AdminLevel = createUserDto.AdminLevel,
            PasswordHash = HashPassword(createUserDto.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapUserToDto(user));
    }

    /// <summary>
    /// Update a user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, CreateUserDto updateUserDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Check if new username exists (excluding current user)
        if (updateUserDto.Username != user.Username && 
            await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username))
            return BadRequest("Username already exists");

        user.Username = updateUserDto.Username;
        user.Email = updateUserDto.Email;
        user.Role = updateUserDto.Role;
        user.AdminLevel = updateUserDto.AdminLevel;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(updateUserDto.Password))
            user.PasswordHash = HashPassword(updateUserDto.Password);

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(MapUserToDto(user));
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Soft delete - mark as inactive
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private UserDto MapUserToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            AdminLevel = user.AdminLevel,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
