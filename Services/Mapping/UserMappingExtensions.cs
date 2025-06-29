using iskxpress_api.Models;
using iskxpress_api.DTOs.Users;

namespace iskxpress_api.Services.Mapping;

/// <summary>
/// Extension methods for mapping between User models and DTOs
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Maps a User model to a UserResponse DTO
    /// </summary>
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Verified = user.Verified,
            AuthProvider = user.AuthProvider,
            Role = user.Role,
            ProfilePictureId = user.ProfilePictureId,
            PictureUrl = user.ProfilePicture?.ObjectUrl
        };
    }

    /// <summary>
    /// Maps a CreateUserRequest DTO to a User model
    /// </summary>
    public static User ToModel(this CreateUserRequest request)
    {
        return new User
        {
            Name = request.Name,
            Email = request.Email,
            Verified = false, // Default to false as requested
            AuthProvider = request.AuthProvider,
            Role = MapUserRole(request.AuthProvider),
            ProfilePictureId = request.ProfilePictureId
        };
    }

    /// <summary>
    /// Maps user role from authentication provider
    /// Microsoft users are regular Users, Google users are Vendors
    /// </summary>
    private static UserRole MapUserRole(AuthProvider authProvider)
    {
        return authProvider switch
        {
            AuthProvider.Microsoft => UserRole.User,
            AuthProvider.Google => UserRole.Vendor,
            _ => throw new ArgumentException($"Unknown auth provider: {authProvider}")
        };
    }

    /// <summary>
    /// Maps an UpdateUserRequest DTO to update an existing User model
    /// </summary>
    public static void UpdateFromRequest(this User user, UpdateUserRequest request)
    {
        user.Name = request.Name;
        user.ProfilePictureId = request.ProfilePictureId;
    }

    /// <summary>
    /// Maps a FirebaseUserSyncRequest DTO to a User model for new users
    /// </summary>
    public static User ToUserModel(this FirebaseUserSyncRequest request, AuthProvider authProvider, UserRole role)
    {
        return new User
        {
            Name = request.Name,
            Email = request.Email,
            Verified = request.Verified,
            AuthProvider = authProvider,
            Role = role
            // Note: PictureURL from Firebase will need to be handled separately to create FileRecord
        };
    }

    /// <summary>
    /// Maps a collection of User models to UserResponse DTOs
    /// </summary>
    public static IEnumerable<UserResponse> ToResponse(this IEnumerable<User> users)
    {
        return users.Select(u => u.ToResponse());
    }
} 