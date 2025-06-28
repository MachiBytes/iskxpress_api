using Xunit;
using Moq;
using FluentAssertions;
using iskxpress_api.Models;
using iskxpress_api.Services;
using iskxpress_api.Repositories;
using iskxpress_api.DTOs.Users;

namespace iskxpress_api.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserRepository.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Verified = true,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            PictureURL = "https://example.com/picture.jpg"
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedUser.Id);
        result.Name.Should().Be(expectedUser.Name);
        result.Email.Should().Be(expectedUser.Email);
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new User
        {
            Id = 1,
            Name = "Test User",
            Email = email,
            Verified = true,
            AuthProvider = AuthProvider.Microsoft,
            Role = UserRole.User
        };

        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        _mockUserRepository.Verify(repo => repo.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ValidRole_ReturnsUsers()
    {
        // Arrange
        var role = UserRole.Vendor;
        var expectedUsers = new List<User>
        {
            new User { Id = 1, Name = "Vendor 1", Email = "vendor1@example.com", Role = UserRole.Vendor },
            new User { Id = 2, Name = "Vendor 2", Email = "vendor2@example.com", Role = UserRole.Vendor }
        };

        _mockUserRepository.Setup(repo => repo.GetByRoleAsync(role))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetUsersByRoleAsync(role);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        resultList[0].Role.Should().Be(UserRole.Vendor);
        resultList[1].Role.Should().Be(UserRole.Vendor);
        _mockUserRepository.Verify(repo => repo.GetByRoleAsync(role), Times.Once);
    }

    [Fact]
    public async Task GetUsersByAuthProviderAsync_ValidProvider_ReturnsUsers()
    {
        // Arrange
        var authProvider = AuthProvider.Google;
        var expectedUsers = new List<User>
        {
            new User { Id = 1, Name = "Google User 1", Email = "user1@gmail.com", AuthProvider = AuthProvider.Google },
            new User { Id = 2, Name = "Google User 2", Email = "user2@gmail.com", AuthProvider = AuthProvider.Google }
        };

        _mockUserRepository.Setup(repo => repo.GetByAuthProviderAsync(authProvider))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetUsersByAuthProviderAsync(authProvider);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        resultList[0].AuthProvider.Should().Be(AuthProvider.Google);
        resultList[1].AuthProvider.Should().Be(AuthProvider.Google);
        _mockUserRepository.Verify(repo => repo.GetByAuthProviderAsync(authProvider), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsCreatedUser()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            Name = "New User",
            Email = "new@example.com",
            AuthProvider = AuthProvider.Microsoft,
            Role = UserRole.User
        };

        var createdUser = new User
        {
            Id = 1,
            Name = createRequest.Name,
            Email = createRequest.Email,
            Verified = false,
            AuthProvider = createRequest.AuthProvider,
            Role = createRequest.Role
        };

        _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be(createRequest.Email);
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUser_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserRequest
        {
            Name = "Updated User",
            PictureURL = "https://example.com/updated.jpg"
        };

        var existingUser = new User
        {
            Id = userId,
            Name = "Original User",
            Email = "original@example.com",
            Verified = false,
            AuthProvider = AuthProvider.Microsoft,
            Role = UserRole.User
        };

        var updatedUser = new User
        {
            Id = userId,
            Name = updateRequest.Name,
            Email = existingUser.Email,
            Verified = existingUser.Verified,
            AuthProvider = existingUser.AuthProvider,
            Role = existingUser.Role,
            PictureURL = updateRequest.PictureURL
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be(updateRequest.Name);
        result.Email.Should().Be(existingUser.Email);
        result.Verified.Should().Be(existingUser.Verified);
        result.AuthProvider.Should().Be(existingUser.AuthProvider);
        result.Role.Should().Be(existingUser.Role);
        result.PictureURL.Should().Be(updateRequest.PictureURL);
        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ExistingUser_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        _mockUserRepository.Setup(repo => repo.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(repo => repo.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(repo => repo.DeleteAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
        _mockUserRepository.Verify(repo => repo.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_ValidCall_ReturnsAllUsers()
    {
        // Arrange
        var expectedUsers = new List<User>
        {
            new User { Id = 1, Name = "User 1", Email = "user1@example.com", Role = UserRole.User },
            new User { Id = 2, Name = "User 2", Email = "user2@example.com", Role = UserRole.Vendor }
        };

        _mockUserRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.Role == UserRole.User || u.Role == UserRole.Vendor);
        _mockUserRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    // Note: SyncAllFirebaseUsersAsync tests would require Firebase Admin SDK mocking
    // which is complex and would need integration tests rather than unit tests
    // due to Firebase's static dependencies
} 