using System;

namespace HomeFitPlanner.Core.Models;

/// <summary>
/// User account credential and profile binding for DSA-driven authentication.
/// </summary>
public class UserAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserProfile Profile { get; set; } = new();
}

