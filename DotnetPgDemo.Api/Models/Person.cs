using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetPgDemo.Api.Models;

public class Person
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
}
