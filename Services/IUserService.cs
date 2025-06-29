using iskxpress_api.Models;
using iskxpress_api.DTOs.Users;
using Microsoft.AspNetCore.Http;

namespace iskxpress_api.Services;

public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
    Task<SyncResultDto> SyncAllFirebaseUsersAsync();
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> UploadProfilePictureAsync(int userId, IFormFile file);
}

 