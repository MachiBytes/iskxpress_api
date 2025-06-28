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
            PictureURL = user.PictureURL
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
            Role = request.Role,
            PictureURL = request.PictureURL
        };
    }

    /// <summary>
    /// Maps an UpdateUserRequest DTO to update an existing User model
    /// </summary>
    public static void UpdateFromRequest(this User user, UpdateUserRequest request)
    {
        user.Name = request.Name;
        user.PictureURL = request.PictureURL;
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
            Role = role,
            PictureURL = request.PictureURL
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