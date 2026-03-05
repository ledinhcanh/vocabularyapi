using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    // --- Gamification ---
    public int XP { get; set; } = 0;
    public int Level { get; set; } = 1;
    public int StreakCount { get; set; } = 0;
    public DateTime? LastStudyDate { get; set; }
}
