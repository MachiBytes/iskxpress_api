using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using iskxpress_api.Controllers;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Services;
using iskxpress_api.Models;

namespace iskxpress_api.Tests;

public class StallControllerTests
{
    private readonly Mock<IStallService> _mockStallService;
    private readonly Mock<ILogger<StallController>> _mockLogger;
    private readonly StallController _controller;

    public StallControllerTests()
    {
        _mockStallService = new Mock<IStallService>();
        _mockLogger = new Mock<ILogger<StallController>>();
        _controller = new StallController(_mockStallService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllStalls_ReturnsOkWithStalls()
    {
        // Arrange
        var expectedStalls = new List<StallResponse>
        {
            new StallResponse { Id = 1, Name = "Stall 1", ShortDescription = "Description 1" },
            new StallResponse { Id = 2, Name = "Stall 2", ShortDescription = "Description 2" }
        };

        _mockStallService.Setup(service => service.GetAllStallsAsync())
            .ReturnsAsync(expectedStalls);

        // Act
        var result = await _controller.GetAllStalls();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stalls = okResult.Value.Should().BeAssignableTo<IEnumerable<StallResponse>>().Subject;
        stalls.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStall_ExistingStall_ReturnsOkWithStall()
    {
        // Arrange
        var stallId = 1;
        var expectedStall = new StallResponse
        {
            Id = stallId,
            Name = "Test Stall",
            ShortDescription = "Test Description"
        };

        _mockStallService.Setup(service => service.GetStallByIdAsync(stallId))
            .ReturnsAsync(expectedStall);

        // Act
        var result = await _controller.GetStall(stallId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stall = okResult.Value.Should().BeOfType<StallResponse>().Subject;
        stall.Id.Should().Be(stallId);
        stall.Name.Should().Be("Test Stall");
    }

    [Fact]
    public async Task GetStall_NonExistingStall_ReturnsNotFound()
    {
        // Arrange
        var stallId = 1;
        _mockStallService.Setup(service => service.GetStallByIdAsync(stallId))
            .ReturnsAsync((StallResponse?)null);

        // Act
        var result = await _controller.GetStall(stallId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Stall with ID {stallId} not found");
    }

    [Fact]
    public async Task GetStallByVendor_ExistingStall_ReturnsOkWithStall()
    {
        // Arrange
        var vendorId = 1;
        var expectedStall = new StallResponse
        {
            Id = 1,
            Name = "Vendor's Stall",
            VendorId = vendorId
        };

        _mockStallService.Setup(service => service.GetStallByVendorIdAsync(vendorId))
            .ReturnsAsync(expectedStall);

        // Act
        var result = await _controller.GetStallByVendor(vendorId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stall = okResult.Value.Should().BeOfType<StallResponse>().Subject;
        stall.Should().BeEquivalentTo(expectedStall);
    }

    [Fact]
    public async Task GetStallByVendor_NonExistingStall_ReturnsNotFound()
    {
        // Arrange
        var vendorId = 999;
        _mockStallService.Setup(service => service.GetStallByVendorIdAsync(vendorId))
            .ReturnsAsync((StallResponse?)null);

        // Act
        var result = await _controller.GetStallByVendor(vendorId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"No stall found for vendor {vendorId}");
    }

    [Fact]
    public async Task CreateStall_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateStallRequest
        {
            VendorId = 1,
            Name = "New Stall",
            ShortDescription = "New Description"
        };

        var createdStall = new StallResponse
        {
            Id = 1,
            Name = request.Name,
            ShortDescription = request.ShortDescription
        };

        _mockStallService.Setup(service => service.CreateStallAsync(request))
            .ReturnsAsync(createdStall);

        // Act
        var result = await _controller.CreateStall(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetStall));
        createdResult.RouteValues!["stallId"].Should().Be(1);
        
        var stall = createdResult.Value.Should().BeOfType<StallResponse>().Subject;
        stall.Name.Should().Be("New Stall");
    }

    [Fact]
    public async Task CreateStall_InvalidRequest_ReturnsConflict()
    {
        // Arrange
        var request = new CreateStallRequest
        {
            VendorId = 1,
            Name = "New Stall",
            ShortDescription = "New Description"
        };

        _mockStallService.Setup(service => service.CreateStallAsync(request))
            .ReturnsAsync((StallResponse?)null);

        // Act
        var result = await _controller.CreateStall(request);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().Be($"Vendor {request.VendorId} already has a stall");
    }

    [Fact]
    public async Task UpdateStall_ValidRequest_ReturnsOkWithUpdatedStall()
    {
        // Arrange
        var stallId = 1;
        var request = new UpdateStallRequest
        {
            Name = "Updated Stall",
            ShortDescription = "Updated Description"
        };

        var updatedStall = new StallResponse
        {
            Id = stallId,
            Name = request.Name,
            ShortDescription = request.ShortDescription
        };

        _mockStallService.Setup(service => service.UpdateStallAsync(stallId, request))
            .ReturnsAsync(updatedStall);

        // Act
        var result = await _controller.UpdateStall(stallId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stall = okResult.Value.Should().BeOfType<StallResponse>().Subject;
        stall.Name.Should().Be("Updated Stall");
    }

    [Fact]
    public async Task UpdateStall_StallNotFound_ReturnsNotFound()
    {
        // Arrange
        var stallId = 1;
        var request = new UpdateStallRequest
        {
            Name = "Updated Stall",
            ShortDescription = "Updated Description"
        };

        _mockStallService.Setup(service => service.UpdateStallAsync(stallId, request))
            .ReturnsAsync((StallResponse?)null);

        // Act
        var result = await _controller.UpdateStall(stallId, request);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Stall with ID {stallId} not found");
    }
}

public class SectionControllerTests
{
    private readonly Mock<ISectionService> _mockSectionService;
    private readonly SectionController _controller;

    public SectionControllerTests()
    {
        _mockSectionService = new Mock<ISectionService>();
        _controller = new SectionController(_mockSectionService.Object);
    }

    [Fact]
    public async Task GetSectionsByStall_ValidStall_ReturnsOkWithSections()
    {
        // Arrange
        var stallId = 1;
        var expectedSections = new List<SectionResponse>
        {
            new SectionResponse { Id = 1, Name = "Appetizers", StallId = stallId },
            new SectionResponse { Id = 2, Name = "Main Courses", StallId = stallId }
        };

        _mockSectionService.Setup(service => service.GetSectionsByStallIdAsync(stallId))
            .ReturnsAsync(expectedSections);

        // Act
        var result = await _controller.GetSectionsByStall(stallId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var sections = okResult.Value.Should().BeAssignableTo<IEnumerable<SectionResponse>>().Subject;
        sections.Should().HaveCount(2);
        sections.First().Name.Should().Be("Appetizers");
        sections.Last().Name.Should().Be("Main Courses");
    }

    [Fact]
    public async Task GetSection_ExistingSection_ReturnsOkWithSection()
    {
        // Arrange
        var sectionId = 1;
        var expectedSection = new SectionResponse
        {
            Id = sectionId,
            Name = "Test Section",
            StallId = 1
        };

        _mockSectionService.Setup(service => service.GetSectionByIdAsync(sectionId))
            .ReturnsAsync(expectedSection);

        // Act
        var result = await _controller.GetSection(sectionId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var section = okResult.Value.Should().BeOfType<SectionResponse>().Subject;
        section.Id.Should().Be(sectionId);
        section.Name.Should().Be("Test Section");
    }

    [Fact]
    public async Task GetSection_NonExistingSection_ReturnsNotFound()
    {
        // Arrange
        var sectionId = 1;
        _mockSectionService.Setup(service => service.GetSectionByIdAsync(sectionId))
            .ReturnsAsync((SectionResponse?)null);

        // Act
        var result = await _controller.GetSection(sectionId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Section with ID {sectionId} not found");
    }

    [Fact]
    public async Task CreateSection_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var stallId = 1;
        var request = new CreateSectionRequest
        {
            Name = "New Section"
        };

        var createdSection = new SectionResponse
        {
            Id = 1,
            Name = request.Name,
            StallId = stallId
        };

        _mockSectionService.Setup(service => service.CreateSectionAsync(stallId, request))
            .ReturnsAsync(createdSection);

        // Act
        var result = await _controller.CreateSection(stallId, request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetSection));
        createdResult.RouteValues!["sectionId"].Should().Be(1);
        
        var section = createdResult.Value.Should().BeOfType<SectionResponse>().Subject;
        section.Name.Should().Be("New Section");
    }

    [Fact]
    public async Task CreateSection_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var stallId = 1;
        var request = new CreateSectionRequest
        {
            Name = "New Section"
        };

        _mockSectionService.Setup(service => service.CreateSectionAsync(stallId, request))
            .ReturnsAsync((SectionResponse?)null);

        // Act
        var result = await _controller.CreateSection(stallId, request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be($"Unable to create section. Stall {stallId} may not exist.");
    }

    [Fact]
    public async Task UpdateSection_ValidRequest_ReturnsOkWithUpdatedSection()
    {
        // Arrange
        var sectionId = 1;
        var request = new UpdateSectionRequest
        {
            Name = "Updated Section"
        };

        var updatedSection = new SectionResponse
        {
            Id = sectionId,
            Name = request.Name,
            StallId = 1
        };

        _mockSectionService.Setup(service => service.UpdateSectionAsync(sectionId, request))
            .ReturnsAsync(updatedSection);

        // Act
        var result = await _controller.UpdateSection(sectionId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var section = okResult.Value.Should().BeOfType<SectionResponse>().Subject;
        section.Name.Should().Be("Updated Section");
    }

    [Fact]
    public async Task UpdateSection_SectionNotFound_ReturnsNotFound()
    {
        // Arrange
        var sectionId = 1;
        var request = new UpdateSectionRequest
        {
            Name = "Updated Section"
        };

        _mockSectionService.Setup(service => service.UpdateSectionAsync(sectionId, request))
            .ReturnsAsync((SectionResponse?)null);

        // Act
        var result = await _controller.UpdateSection(sectionId, request);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Section with ID {sectionId} not found");
    }

    [Fact]
    public async Task DeleteSection_ExistingSection_ReturnsNoContent()
    {
        // Arrange
        var sectionId = 1;
        _mockSectionService.Setup(service => service.DeleteSectionAsync(sectionId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSection(sectionId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteSection_SectionNotFound_ReturnsNotFound()
    {
        // Arrange
        var sectionId = 1;
        _mockSectionService.Setup(service => service.DeleteSectionAsync(sectionId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteSection(sectionId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Section with ID {sectionId} not found");
    }
} 