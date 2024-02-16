using Auth.AuthenticationService.Services.Auth0;
using Auth.Models.Dtos;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;

namespace Auth.Test.UserTest;

public class Auth0UserTest
{
    private readonly Auth0UserService _service;
    private readonly Mock<IAuthenticationApiClient> _client;
    private readonly Mock<IManagementService> _managementService;

    public Auth0UserTest()
    {
        _client = new();
        Mock<ILogger<Auth0UserService>> logger = new();
        _managementService = new();
        _service = new Auth0UserService(_managementService.Object, logger.Object, _client.Object);
    }

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_SamePassword_ThrowsArgumentException()
    {
        // Arrange
        var idToken = "mockIdToken";
        var changePassword = new ChangePasswordDto
        {
            Password = "password",
            NewPassword = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ChangePassword(idToken, changePassword));
    }

    [Fact]
    public async Task ChangePassword_EmailMismatch_ThrowsArgumentException()
    {
        // Arrange
        var idToken = "mockIdToken";
        var changePassword = new ChangePasswordDto
        {
            Password = "password",
            NewPassword = "newPassword",
            Email = "test-1@gmail.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ChangePassword(idToken, changePassword));
    }

    [Fact]
    public async Task ChangePassword_ManagementApiUpdateFails_ThrowsException()
    {
        // Arrange
        var idToken = "mockIdToken";
        var changePassword = new ChangePasswordDto
        {
            Password = "password",
            NewPassword = "newPassword",
            Email = "test@test.com"
        };

        // Mock the management API client to throw an exception during update
        _managementService.Setup(m => m.GetManagmentApiClient()).Throws(new Exception("Mock update failure"));
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.ChangePassword(idToken, changePassword));
    }

    #endregion

    #region GetUser

    [Fact]
    public async Task User_ValidToken_ReturnsUserInfo()
    {
        // Arrange
        var token = "valid_token";
        var userInfoDto = new UserInfo() { UserId = "user123" };
        _client
            .Setup(x => x.GetUserInfoAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInfoDto);
        // Act
        var result = await _service.User(token);

        // Assert
        Assert.Equal(userInfoDto.UserId, result.UserId);
    }

    [Fact]
    public async Task User_InvalidToken_ThrowsException()
    {
        // Arrange
        var token = "invalid_token";
        _client.Setup(x => x.GetUserInfoAsync(token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid token"));

        // Assert
        await Assert.ThrowsAsync<Exception>(() => _service.User(token));
    }

    [Fact]
    public async Task User_ExceptionThrownByGetUserInfoAsync_ThrowsException()
    {
        // Arrange
        var token = "valid_token";
        _client.Setup(x => x.GetUserInfoAsync(token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error fetching user info"));
        var loggerMock = new Mock<ILogger>();

        // Assert
        await Assert.ThrowsAsync<Exception>(() => _service.User(token));
    }

    #endregion

    #region UpdateUser

    [Fact]
    public async Task UpdateUser_ValidTokenAndInfo_Success()
    {
        // Arrange
        var idToken = "validToken";
        var userInfoDto = new UserInfoDto { NickName = "newNick" };

        // Act
        var result = await _service.UpdateUser(idToken, userInfoDto);

        // Assert
        Assert.Equal("User {userId} Updated successfull", result);
    }

    [Fact]
    public async Task UpdateUser_InvalidToken_ExceptionThrown()
    {
        // Arrange
        var idToken = "invalidToken";
        var userInfoDto = new UserInfoDto { NickName = "newNick" };

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => _service.UpdateUser(idToken, userInfoDto));
    }

    [Fact]
    public async Task UpdateUser_NullUserInfo_ExceptionThrown()
    {
        // Arrange
        var idToken = "validToken";
        UserInfoDto userInfoDto = null;

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => _service.UpdateUser(idToken, userInfoDto));
    }

    #endregion
}