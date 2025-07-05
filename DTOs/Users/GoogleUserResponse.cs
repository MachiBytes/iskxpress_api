namespace iskxpress_api.DTOs.Users;

/// <summary>
/// Response DTO for Google users from Firebase Auth
/// </summary>
public class GoogleUserResponse
{
    /// <summary>
    /// Firebase UID
    /// </summary>
    public string Uid { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User's profile picture URL
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Whether the email is verified
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the user last signed in
    /// </summary>
    public DateTime? LastSignInAt { get; set; }

    /// <summary>
    /// Whether the user account is disabled
    /// </summary>
    public bool Disabled { get; set; }
} 