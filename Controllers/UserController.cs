using Microsoft.AspNetCore.Mvc;
using iskxpress_api.Models;
using iskxpress_api.Services;
using iskxpress_api.DTOs.Users;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users in the system</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound($"User with email {email} not found");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get users by their role
    /// </summary>
    /// <param name="role">The user role (User or Vendor)</param>
    /// <returns>List of users with the specified role</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("role/{role}")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsersByRole(UserRole role)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with role {Role}", role);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get users by their authentication provider
    /// </summary>
    /// <param name="provider">The authentication provider (Google or Microsoft)</param>
    /// <returns>List of users using the specified auth provider</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("provider/{provider}")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsersByAuthProvider(AuthProvider provider)
    {
        try
        {
            var users = await _userService.GetUsersByAuthProviderAsync(provider);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with auth provider {Provider}", provider);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Sync all Firebase users with local database
    /// </summary>
    /// <remarks>
    /// This endpoint connects directly to Firebase Authentication and syncs all users to the local database.
    /// - Microsoft SSO users become regular Users
    /// - Google SSO users become Vendors
    /// - Existing users are updated with latest Firebase data
    /// - New users are created with appropriate roles
    /// 
    /// Requires firebase-key.json file in the project root.
    /// </remarks>
    /// <returns>Detailed sync operation results</returns>
    /// <response code="200">Returns sync operation statistics</response>
    /// <response code="500">Internal server error or Firebase connection issue</response>
    [HttpGet("sync")]
    [ProducesResponseType(typeof(SyncResultDto), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<SyncResultDto>> SyncAllFirebaseUsers()
    {
        try
        {
            var syncResult = await _userService.SyncAllFirebaseUsersAsync();
            
            _logger.LogInformation("Firebase sync completed: {TotalProcessed} processed, {NewUsers} new, {UpdatedUsers} updated, {ErrorsCount} errors", 
                syncResult.TotalProcessed, syncResult.NewUsers, syncResult.UpdatedUsers, syncResult.ErrorsCount);

            if (syncResult.ErrorsCount > 0)
            {
                _logger.LogWarning("Sync completed with errors: {Errors}", string.Join("; ", syncResult.Errors));
            }

            return Ok(syncResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Firebase sync");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request data</param>
    /// <returns>Created user details</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="request">User update request data</param>
    /// <returns>Updated user details</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUser = await _userService.UpdateUserAsync(id, request);
            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "User not found for update: {UserId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">User deleted successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound($"User with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
} 