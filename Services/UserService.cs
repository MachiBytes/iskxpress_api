using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.DTOs.Users;
using iskxpress_api.Services.Mapping;
using FirebaseAdmin.Auth;

namespace iskxpress_api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user?.ToResponse();
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user?.ToResponse();
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = request.ToModel();
        var createdUser = await _userRepository.AddAsync(user);
        return createdUser.ToResponse();
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            throw new ArgumentException($"User with ID {id} not found");
        }

        existingUser.UpdateFromRequest(request);
        var updatedUser = await _userRepository.UpdateAsync(existingUser);
        return updatedUser.ToResponse();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.ToResponse();
    }

    public async Task<SyncResultDto> SyncAllFirebaseUsersAsync()
    {
        var result = new SyncResultDto();

        try
        {
            // Initialize Firebase if not already done
            FirebaseInitializer.InitializeFirebase();

            // Get all users from Firebase
            var pagedEnumerable = FirebaseAuth.DefaultInstance.ListUsersAsync(null);
            var users = new List<ExportedUserRecord>();

            var enumerator = pagedEnumerable.GetAsyncEnumerator();
            await foreach (var user in pagedEnumerable)
            {
                users.Add(user);
            }

            result.TotalProcessed = users.Count;

            foreach (var firebaseUser in users)
            {
                try
                {
                    await SyncSingleUserAsync(firebaseUser, result);
                }
                catch (Exception ex)
                {
                    result.ErrorsCount++;
                    result.Errors.Add($"Error syncing user {firebaseUser.Email}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            result.ErrorsCount++;
            result.Errors.Add($"General sync error: {ex.Message}");
        }

        return result;
    }

    private async Task SyncSingleUserAsync(ExportedUserRecord firebaseUser, SyncResultDto result)
    {
        if (string.IsNullOrEmpty(firebaseUser.Email))
        {
            result.ErrorsCount++;
            result.Errors.Add($"User {firebaseUser.Uid} has no email address");
            return;
        }

        // Check if user already exists in our database
        var existingUser = await _userRepository.GetByEmailAsync(firebaseUser.Email);

        if (existingUser != null)
        {
            // Only update verification status for existing users, preserve name and picture
            existingUser.Verified = firebaseUser.EmailVerified;
            
            await _userRepository.UpdateAsync(existingUser);
            result.UpdatedUsers++;
        }
        else
        {
            // Determine auth provider from Firebase user providers
            var authProvider = DetermineAuthProvider(firebaseUser);
            if (authProvider == null)
            {
                result.ErrorsCount++;
                result.Errors.Add($"Could not determine auth provider for user {firebaseUser.Email}");
                return;
            }

            // Create new user
            var newUser = new User
            {
                Name = firebaseUser.DisplayName ?? firebaseUser.Email,
                Email = firebaseUser.Email,
                // Firebase picture URL will need to be handled separately to create FileRecord
                Verified = firebaseUser.EmailVerified,
                AuthProvider = authProvider.Value,
                Role = MapUserRole(authProvider.Value)
            };

            await _userRepository.AddAsync(newUser);
            result.NewUsers++;
        }
    }

    private AuthProvider? DetermineAuthProvider(ExportedUserRecord firebaseUser)
    {
        // Check provider data to determine auth provider
        foreach (var provider in firebaseUser.ProviderData)
        {
            if (provider.ProviderId.Contains("google"))
                return AuthProvider.Google;
            if (provider.ProviderId.Contains("microsoft"))
                return AuthProvider.Microsoft;
        }

        // Fallback: check email domain
        if (firebaseUser.Email?.EndsWith("@gmail.com") == true)
            return AuthProvider.Google;
        if (firebaseUser.Email?.EndsWith("@outlook.com") == true || 
            firebaseUser.Email?.EndsWith("@hotmail.com") == true ||
            firebaseUser.Email?.EndsWith("@live.com") == true)
            return AuthProvider.Microsoft;

        return null; // Unknown provider
    }

    private UserRole MapUserRole(AuthProvider authProvider)
    {
        // Microsoft users are regular Users, Google users are Vendors
        return authProvider switch
        {
            AuthProvider.Microsoft => UserRole.User,
            AuthProvider.Google => UserRole.Vendor,
            _ => throw new ArgumentException($"Unknown auth provider: {authProvider}")
        };
    }
} 