using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using iskxpress_api.Models;
using iskxpress_api.Services;
using iskxpress_api.Controllers;
using iskxpress_api.DTOs.Users;

namespace iskxpress_api.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _controller = new UserController(_mockUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllUsers_ValidRequest_ReturnsOkWithUsers()
    {
        // Arrange
        var expectedUsers = new List<UserResponse>
        {
            new UserResponse { Id = 1, Name = "User 1", Email = "user1@example.com", Role = UserRole.User },
            new UserResponse { Id = 2, Name = "User 2", Email = "user2@example.com", Role = UserRole.Vendor }
        };

        _mockUserService.Setup(service => service.GetAllUsersAsync())
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var users = okResult.Value.Should().BeAssignableTo<IEnumerable<UserResponse>>().Subject;
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllUsers_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockUserService.Setup(service => service.GetAllUsersAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("Internal server error");
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsOkWithUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new UserResponse
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Verified = true,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor
        };

        _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var user = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        user.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
            .ReturnsAsync((UserResponse?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"User with ID {userId} not found");
    }

    [Fact]
    public async Task GetUserByEmail_ExistingUser_ReturnsOkWithUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new UserResponse
        {
            Id = 1,
            Name = "Test User",
            Email = email,
            Verified = true,
            AuthProvider = AuthProvider.Microsoft,
            Role = UserRole.User
        };

        _mockUserService.Setup(service => service.GetUserByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var user = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        user.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmail_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserService.Setup(service => service.GetUserByEmailAsync(email))
            .ReturnsAsync((UserResponse?)null);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"User with email {email} not found");
    }

    [Fact]
    public async Task GetUsersByRole_ValidRole_ReturnsOkWithUsers()
    {
        // Arrange
        var role = UserRole.Vendor;
        var expectedUsers = new List<UserResponse>
        {
            new UserResponse { Id = 1, Name = "Vendor 1", Email = "vendor1@example.com", Role = UserRole.Vendor },
            new UserResponse { Id = 2, Name = "Vendor 2", Email = "vendor2@example.com", Role = UserRole.Vendor }
        };

        _mockUserService.Setup(service => service.GetUsersByRoleAsync(role))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _controller.GetUsersByRole(role);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var users = okResult.Value.Should().BeAssignableTo<IEnumerable<UserResponse>>().Subject;
        users.Should().HaveCount(2);
        users.Should().OnlyContain(u => u.Role == UserRole.Vendor);
    }

    [Fact]
    public async Task GetUsersByAuthProvider_ValidProvider_ReturnsOkWithUsers()
    {
        // Arrange
        var authProvider = AuthProvider.Google;
        var expectedUsers = new List<UserResponse>
        {
            new UserResponse { Id = 1, Name = "Google User 1", Email = "user1@gmail.com", AuthProvider = AuthProvider.Google },
            new UserResponse { Id = 2, Name = "Google User 2", Email = "user2@gmail.com", AuthProvider = AuthProvider.Google }
        };

        _mockUserService.Setup(service => service.GetUsersByAuthProviderAsync(authProvider))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _controller.GetUsersByAuthProvider(authProvider);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var users = okResult.Value.Should().BeAssignableTo<IEnumerable<UserResponse>>().Subject;
        users.Should().HaveCount(2);
        users.Should().OnlyContain(u => u.AuthProvider == AuthProvider.Google);
    }

    [Fact]
    public async Task SyncAllFirebaseUsers_ValidRequest_ReturnsOkWithSyncResult()
    {
        // Arrange
        var expectedSyncResult = new SyncResultDto
        {
            TotalProcessed = 10,
            NewUsers = 3,
            UpdatedUsers = 7,
            ErrorsCount = 0,
            Errors = new List<string>()
        };

        _mockUserService.Setup(service => service.SyncAllFirebaseUsersAsync())
            .ReturnsAsync(expectedSyncResult);

        // Act
        var result = await _controller.SyncAllFirebaseUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var syncResult = okResult.Value.Should().BeOfType<SyncResultDto>().Subject;
        syncResult.Should().BeEquivalentTo(expectedSyncResult);
    }

    [Fact]
    public async Task SyncAllFirebaseUsers_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockUserService.Setup(service => service.SyncAllFirebaseUsersAsync())
            .ThrowsAsync(new Exception("Firebase connection error"));

        // Act
        var result = await _controller.SyncAllFirebaseUsers();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("Internal server error");
    }

    [Fact]
    public async Task CreateUser_ValidUser_ReturnsCreatedAtAction()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            Name = "New User",
            Email = "new@example.com",
            AuthProvider = AuthProvider.Microsoft,
            Role = UserRole.User
        };

        var createdUser = new UserResponse
        {
            Id = 1,
            Name = createRequest.Name,
            Email = createRequest.Email,
            Verified = false,
            AuthProvider = createRequest.AuthProvider,
            Role = createRequest.Role
        };

        _mockUserService.Setup(service => service.CreateUserAsync(createRequest))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(createRequest);

        // Assert
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be("GetUser");
        createdAtActionResult.RouteValues!["id"].Should().Be(createdUser.Id);
        var returnedUser = createdAtActionResult.Value.Should().BeOfType<UserResponse>().Subject;
        returnedUser.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task UpdateUser_ValidUser_ReturnsOkWithUpdatedUser()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserRequest
        {
            Name = "Updated User",
            PictureURL = "https://example.com/updated.jpg"
        };

        var updatedUser = new UserResponse
        {
            Id = userId,
            Name = updateRequest.Name,
            Email = "existing@example.com",
            Verified = true,
            AuthProvider = AuthProvider.Microsoft,
            Role = UserRole.User,
            PictureURL = updateRequest.PictureURL
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        returnedUser.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task UpdateUser_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserRequest
        {
            Name = "", // Invalid - empty name
            PictureURL = "https://example.com/picture.jpg"
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        var updateRequest = new UpdateUserRequest
        {
            Name = "Updated User",
            PictureURL = "https://example.com/updated.jpg"
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ThrowsAsync(new ArgumentException($"User with ID {userId} not found"));

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"User with ID {userId} not found");
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        _mockUserService.Setup(service => service.DeleteUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _mockUserService.Setup(service => service.DeleteUserAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"User with ID {userId} not found");
    }
} 