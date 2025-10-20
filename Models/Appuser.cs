using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Appuser
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Avatarurl { get; set; }

    public string? Status { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpiresAt { get; set; }

    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? LastOtpSentAt { get; set; }

    public DateTime? LastVerificationEmailSentAt { get; set; }

    public virtual ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
}
