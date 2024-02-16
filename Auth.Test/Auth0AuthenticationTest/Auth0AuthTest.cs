using System.Net;
using Auth.AuthenticationService.Services.Auth0;
using Auth.Models.Dtos;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.Test.Auth0AuthenticationTest;

public class Auth0AuthTest
{
    private readonly Mock<IManagementService> _managementService;
    private readonly Mock<IAuthenticationApiClient> _authenticationApiClient;
    private readonly Mock<IMFAService> _mfaService;
    private readonly Auth0Service _service;
    public Auth0AuthTest()
    {
        var logger = new Mock<ILogger<Auth0Service>>();
        _authenticationApiClient = new();
        _managementService = new();
        _mfaService = new ();
        _service = new Auth0Service(_mfaService.Object, _managementService.Object, logger.Object);
    }

    #region SignUpWithMFAAsync

    [Fact]
public async Task SignUpWithMFAAsync_UniquePhoneNumber_Success()
{
    // Arrange
    var signUpDto = new SignUpDto
    {
        Email = "test@example.com",
        Password = "password123",
        UserName = "testuser",
        PhoneNumber = "1234567890"
    };

    var expectedResponse = new AccessTokenDto
    {
        AccessToken = "testAccessToken",
        NeedsMFA = false
    };

    _managementService.Setup(m => m.IsPhoneNumberUnique(signUpDto.PhoneNumber)).ReturnsAsync(true);
    _authenticationApiClient.Setup(m => m.SignupUserAsync(It.IsAny<SignupUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SignupUserResponse());
    _mfaService.Setup(m => m.EnrollPhoneNumberAsync(It.IsAny<string>(), signUpDto.PhoneNumber)).ReturnsAsync("testChallengeId");

    // Act
    var result = await _service.SignUpWithMFAAsync(signUpDto);

    // Assert
    Assert.Equal(expectedResponse.AccessToken, result.AccessToken);
    Assert.Equal(expectedResponse.NeedsMFA, result.NeedsMFA);
}

[Fact]
public async Task SignUpWithMFAAsync_DuplicatePhoneNumber_ExceptionThrown()
{
    // Arrange
    var signUpDto = new SignUpDto
    {
        Email = "test@example.com",
        Password = "password123",
        UserName = "testuser",
        PhoneNumber = "1234567890"
    };

    _managementService.Setup(m => m.IsPhoneNumberUnique(signUpDto.PhoneNumber)).ReturnsAsync(false);


    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(async () => await _service.SignUpWithMFAAsync(signUpDto));
}

[Fact]
public async Task SignUpWithMFAAsync_SuccessfulSignupWithMFA_Success()
{
    // Arrange
    var signUpDto = new SignUpDto
    {
        Email = "test@example.com",
        Password = "password123",
        UserName = "testuser",
        PhoneNumber = "1234567890"
    };

    var expectedResponse = new AccessTokenDto
    {
        AccessToken = null,
        NeedsMFA = true,
        ChallengeId = "testChallengeId"
    };

    _managementService.Setup(m => m.IsPhoneNumberUnique(signUpDto.PhoneNumber)).ReturnsAsync(true);

    _authenticationApiClient.Setup(m => m.SignupUserAsync(It.IsAny<SignupUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SignupUserResponse());

    _mfaService.Setup(m => m.EnrollPhoneNumberAsync(It.IsAny<string>(), signUpDto.PhoneNumber)).ReturnsAsync("testChallengeId");
    
    // Act
    var result = await _service.SignUpWithMFAAsync(signUpDto);

    // Assert
    Assert.Equal(expectedResponse.AccessToken, result.AccessToken);
    Assert.Equal(expectedResponse.NeedsMFA, result.NeedsMFA);
    Assert.Equal(expectedResponse.ChallengeId, result.ChallengeId);
}

[Fact]
public async Task SignUpWithMFAAsync_UnsuccessfulSignupWithMFA_ExceptionThrown()
{
    // Arrange
    var signUpDto = new SignUpDto
    {
        Email = "test@example.com",
        Password = "password123",
        UserName = "testuser",
        PhoneNumber = "1234567890"
    };
    _managementService.Setup(m => m.IsPhoneNumberUnique(signUpDto.PhoneNumber)).ReturnsAsync(true);
    _authenticationApiClient.Setup(m => m.SignupUserAsync(It.IsAny<SignupUserRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ErrorApiException( HttpStatusCode.BadRequest, new ApiError { Error = "testError" }));
    
    // Act & Assert
    await Assert.ThrowsAsync<ErrorApiException>(async () => await _service.SignUpWithMFAAsync(signUpDto));
}
    

    #endregion

    #region SignInWithMFAAsync

    [Fact]
    public async Task TestSignInWithoutMFA()
    {
        // Arrange
        var signInDto = new SignInDto { Email = "test@example.com", Password = "password123" };
        var accessTokenDto = new AccessTokenDto { AccessToken = "token123" };
        _mfaService.Setup(m => m.InitiateMfaChallengeAsync(It.IsAny<string>())).ReturnsAsync("challenge123");
    
        // Act
        var result = await _service.SignInWithMFAAsync(signInDto);
    
        // Assert
        Assert.False(result.NeedsMFA);
        Assert.Equal("token123", result.AccessToken);
    }

    [Fact]
    public async Task TestSignInWithMFA()
    {
        // Arrange
        var signInDto = new SignInDto { Email = "test@example.com", Password = "password123" };
        var accessTokenDto = new AccessTokenDto { MFAToken = "mfa123" };
        _mfaService.Setup(m => m.InitiateMfaChallengeAsync(It.IsAny<string>())).ReturnsAsync("challenge123");
    
        // Act
        var result = await _service.SignInWithMFAAsync(signInDto);
    
        // Assert
        Assert.True(result.NeedsMFA);
        Assert.Equal("challenge123", result.ChallengeId);
    }

    [Fact]
    public async Task TestSignInExceptionHandling()
    {
        // Arrange
        var signInDto = new SignInDto { Email = "test@example.com", Password = "password123" };
    
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.SignInWithMFAAsync(signInDto));
    }

    #endregion

    #region VerifyMfaChallengeAsync 
    [Fact]
    public async Task VerifyMfaChallengeAsync_ValidToken_ReturnsAccessToken()
    {
        // Arrange
        var mfaToken = "validMfaToken";
        var challengeId = "validChallengeId";
        var verifyChallenge = new VerifyChallengeDto { Otp = "123456" };
        var expectedResult = new AccessTokenDto();

        _mfaService.Setup(x => x.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge.Otp))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task VerifyMfaChallengeAsync_InvalidToken_ThrowsException()
    {
        // Arrange
        var mfaToken = "invalidMfaToken";
        var challengeId = "validChallengeId";
        var verifyChallenge = new VerifyChallengeDto { Otp = "123456" };

        _mfaService.Setup(x => x.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge.Otp))
            .ThrowsAsync(new Exception("Invalid MFA token"));

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _service.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge));
    }
    #endregion

    #region SignOutAsync
    [Fact]
    public async Task SignOutAsync_Success()
    {
        // Arrange


        // Act
        var result = await _service.SignOutAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(Uri.IsWellFormedUriString(result, UriKind.Absolute));
    }

    [Fact]
    public async Task SignOutAsync_Exception()
    {
        // Arrange


        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await _service.SignOutAsync());
    }

    #endregion
    
    #region ResetPasswordAsync
        [Fact]
        public async Task ResetPasswordAsync_ValidEmail_ReturnsResult()
        {
            // Arrange
            var email = "test@example.com";
            
            _authenticationApiClient.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync("Success");

            // Act
            var result = await _service.ResetPasswordAsync(email);

            // Assert
            Assert.Equal("Success", result);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidEmail_ThrowsException()
        {
            // Arrange
            var email = "invalidemail";
        
            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ResetPasswordAsync(email));
        }

        [Fact]
        public async Task ResetPasswordAsync_ExceptionThrown_ThrowsException()
        {
            // Arrange
            var email = "test@example.com";
      
            _authenticationApiClient.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Error"));

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ResetPasswordAsync(email));
        }
    

    #endregion
}