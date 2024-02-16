using System.Net;
using Auth.AuthenticationService.Services.IdentityServer;
using Auth.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RestSharp;

namespace Auth.Test.IdentityServiceAuthenticationTest;

public class IdentityServiceAuthTest
{
    private readonly Mock<ILogger<IdentityService>> _loggerMock;
    private readonly Mock<IRestClient> _clientMock;
    private readonly IdentityService _identityService;

    public IdentityServiceAuthTest()
    {
        _loggerMock = new Mock<ILogger<IdentityService>>();
        _clientMock = new Mock<IRestClient>();
        _identityService = new IdentityService(_loggerMock.Object, _clientMock.Object);
    }

    #region SignUpWithMFAAsync

    [Fact]
    public async Task SignUpWithMFAAsync_Success_ReturnsAccessTokenDto()
    {
        // Arrange
        var signUpDto = new SignUpDto();
        var accessTokenDto = new AccessTokenDto();
        var response = new RestResponse { Content = JsonConvert.SerializeObject(accessTokenDto) };
        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _identityService.SignUpWithMFAAsync(signUpDto);

        // Assert
        Assert.Equal(accessTokenDto, result);
    }

    [Fact]
    public async Task SignUpWithMFAAsync_Failure_ThrowsException()
    {
        // Arrange
        var signUpDto = new SignUpDto();
        var response = new RestResponse { IsSuccessStatusCode = false, Content = "error message" };
        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _identityService.SignUpWithMFAAsync(signUpDto));
    }

    #endregion

    #region SignInWithMFAAsync

    [Fact]
    public async Task SignInWithMFASuccessful()
    {
        // Arrange
        var signInDto = new SignInDto
        {
            Email = "test@example.com",
            Password = "password"
        };

        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse { IsSuccessStatusCode = true, Content = "{\"access_token\":\"token\"}" });


        // Act
        var result = await _identityService.SignInWithMFAAsync(signInDto);

        // Assert
        Assert.Equal("token", result.AccessToken);
    }

    [Fact]
    public async Task SignInWithMFAMFARequired()
    {
        // Arrange
        var signInDto = new SignInDto
        {
            Email = "test@example.com",
            Password = "password"
        };

        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse
            {
                Content = "{\"error_description\":\"mfa_required\"}",
                Cookies = new CookieCollection
                    { new Cookie { Name = "Identity.TwoFactorUserId", Value = "mfa_token" } }
            });


        // Act
        var result = await _identityService.SignInWithMFAAsync(signInDto);

        // Assert
        Assert.True(result.NeedsMFA);
        Assert.Equal("mfa_token", result.MFAToken);
    }

    [Fact]
    public async Task SignInWithMFAUnsuccessful()
    {
        // Arrange
        var signInDto = new SignInDto
        {
            Email = "test@example.com",
            Password = "password"
        };

        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse
                { IsSuccessStatusCode = false, ErrorMessage = "Invalid username or password" });


        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _identityService.SignInWithMFAAsync(signInDto));
    }

    #endregion

    #region VerifyMFAAsync

    // Unit tests for VerifyMfaChallengeAsync
    [Fact]
    public async Task VerifyMfaChallengeAsync_SuccessfulVerification_ReturnsAccessTokenDto()
    {
        // Arrange
        var mfaToken = "mockMfaToken";
        var challengeId = "mockChallengeId";
        var verifyChallenge = new VerifyChallengeDto { Otp = "123456" };

        // Act
        var result = await _identityService.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task VerifyMfaChallengeAsync_UnsuccessfulVerification_ThrowsArgumentException()
    {
        // Arrange
        var mfaToken = "mockMfaToken";
        var challengeId = "mockChallengeId";
        var verifyChallenge = new VerifyChallengeDto { Otp = "123456" };

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _identityService.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge);
        });
    }

    [Fact]
    public async Task VerifyMfaChallengeAsync_ExceptionThrownDuringVerification_ThrowsException()
    {
        // Arrange
        var mfaToken = "mockMfaToken";
        var challengeId = "mockChallengeId";
        var verifyChallenge = new VerifyChallengeDto { Otp = "123456" };

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _identityService.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge);
        });
    }



    #endregion

    #region ResetPasswordAsync
    [Fact]
    public async Task ResetPasswordAsync_SuccessfulRequest_ReturnsSuccessMessage()
    {
        // Arrange
        var email = "test@example.com";
        var expectedMessage = "Password Reset mail Sent";
        var responseContent = "some success response";
        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse { IsSuccessStatusCode = true, ResponseStatus = ResponseStatus.Completed, Content = responseContent });

        // Act
        var result = await _identityService.ResetPasswordAsync(email);

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public async Task ResetPasswordAsync_FailedRequest_ThrowsException()
    {
        // Arrange
        var email = "test@example.com";
        var responseContent = "some error response";
        _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, Content = responseContent });

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(async () => await _identityService.ResetPasswordAsync(email));
    }
    

    #endregion

    #region SignOutAsync
    [Fact]
    public async Task SignOutAsync_CreatesCorrectRequest()
    {
        // Arrange
        // Act
        await _identityService.SignOutAsync();

        // Assert
        _clientMock.Verify(c => c.ExecuteAsync(It.Is<RestRequest>(
            r => r.Resource == "your-logout-url" &&
                 r.Method == Method.Post &&
                 r.Parameters.Any(p => p.Name == "id_token_hint" && p.Value.ToString() == "YOUR_ID_TOKEN") &&
                 r.Parameters.Any(p => p.Name == "post_logout_redirect_uri" && p.Value.ToString() == "https://your-app-url/logout") &&
                 r.Parameters.Any(p => p.Name == "Content-Type" && p.Value.ToString() == "application/x-www-form-urlencoded")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_HandlesExceptionsAndLogsError()
    {
        // Arrange
       _clientMock.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _identityService.SignOutAsync());
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error Logging out of Auth.IdentityServer"), Times.Once);
    }

    #endregion

    #region GetValidAccessToken

        [Fact]
        public async Task GetValidAccessToken_ValidRefreshToken_ReturnsAccessToken()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";

            // Act
            var result = await _identityService.GetValidAccessToken(refreshToken);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetValidAccessToken_InvalidRefreshToken_ThrowsArgumentException()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _identityService.GetValidAccessToken(refreshToken));
        }

        [Fact]
        public async Task GetValidAccessToken_SuccessfulTokenRefresh_ReturnsAccessToken()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";

            // Act
            AccessTokenDto result = await _identityService.GetValidAccessToken(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("expected_access_token_value", result.AccessToken);
        }

        [Fact]
        public async Task GetValidAccessToken_NetworkIssues_ThrowsException()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            // Simulate network issue

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(async () => await _identityService.GetValidAccessToken(refreshToken));
        }
    

    #endregion
    
}

