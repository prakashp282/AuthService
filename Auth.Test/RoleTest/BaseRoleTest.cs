using Auth.AuthenticationService.Services.IdentityServer;
using Auth.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;

namespace Auth.Test.RoleTest;

public class BaseRoleTest
{
    //All unit test related to role services
    private readonly RoleService _service;
    private readonly Mock<IRestClient> _client;
    private readonly Mock<ILogger<RoleService>> _logger;

    public BaseRoleTest()
    {
        _client = new Mock<IRestClient>();
        _logger = new Mock<ILogger<RoleService>>();
        _service = new RoleService(_logger.Object, _client.Object);
    }

    #region AddRole

    [Fact]
    public async Task AddRole_Successful()
    {
        // Arrange
        var userRole = new UserRoleDto("Test@test.com", "Admin");
        var expectedResponse = "Role Added!";

        _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new RestResponse
                { IsSuccessStatusCode = true, ResponseStatus = ResponseStatus.Completed });

        // Act
        var result = await _service.AddRole(userRole);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task AddRole_Unsuccessful()
    {
        // Arrange
        var userRole = new UserRoleDto("Test@test.com", "Admin");

        var errorResponse = "Some error message";
        _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new RestResponse
                { IsSuccessStatusCode = false, Content = errorResponse });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.AddRole(userRole));
    }

    [Fact]
    public async Task AddRole_NullUserRole_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddRole(null));
    }

    #endregion
    
    #region RemoveRole
    [Fact]
    public async Task TestRemoveRole_Success()
    {
        // Arrange
        var userRole = new UserRoleDto("Test@test.com", "Admin");
        // Act
        var result = await _service.RemoveRole(userRole);

        // Assert
        Assert.Equal("Role Removed!", result);
    }

    [Fact]
    public async Task TestRemoveRole_Failure()
    {
        // Arrange
        var userRole = new UserRoleDto("Test@test.com", "Admin");
        _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, Content = "Error message" });

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => _service.RemoveRole(userRole));
    }
    #endregion

}


    
    
