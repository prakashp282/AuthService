using System;
using System.Threading.Tasks;
using Auth.AuthenticationService.Services.Common;
using Auth.AuthenticationService.Services.IdentityServer;
using Auth.Models.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Moq;
using RestSharp;
using Xunit;
namespace Auth.Test.UserTest;

public class IdentityServerUserTest
{
        private readonly IdentityUserService _service;
        private readonly Mock<IRestClient> _client;
        private readonly Mock<ILogger<IdentityUserService>> _logger;

        public IdentityServerUserTest()
        {
            _client = new Mock<IRestClient>();
            _logger = new Mock<ILogger<IdentityUserService>>();
            _service = new IdentityUserService(_logger.Object, _client.Object);
        }

        #region GetUser
        [Fact]
        public async Task User_ValidToken_ReturnsUserInfoDto()
        {
            // Arrange
            var token = "valid_token";
            var expectedUserInfo = new UserInfoDto();

            _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { IsSuccessStatusCode = true, Content = JsonConvert.SerializeObject(expectedUserInfo) });

            // Act
            var result = await _service.User(token);

            // Assert
            Assert.Equal(expectedUserInfo, result);
        }

        [Fact]
        public async Task User_InvalidToken_ThrowsArgumentException()
        {
            // Arrange
            var token = "invalid_token";

            _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(),It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, Content = "error message" });

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _service.User(token));
        }

        [Fact]
        public async Task User_NetworkError_ThrowsException()
        {
            // Arrange
            var token = "valid_token";
            _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(),It.IsAny<CancellationToken>())).Throws(new Exception("Network error"));

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(async () => await _service.User(token));
        }
        #endregion

        #region ChangePassword

        [Fact]
        public async Task ChangePassword_SuccessfulResponse_ReturnsSuccessMessage()
        {
            // Arrange
            var idToken = "exampleIdToken";
            var changePasswordDto = new ChangePasswordDto { /* fill in with appropriate data */ };

            _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { IsSuccessStatusCode = true, ResponseStatus = ResponseStatus.Completed, Content = "sampleContent" });
            // Act
            var result = await _service.ChangePassword(idToken, changePasswordDto);

            // Assert
            Assert.Equal("Password Updated Successfully", result);
        }

        [Fact]
        public async Task ChangePassword_UnsuccessfulResponse_ThrowsException()
        {
            // Arrange
            var idToken = "exampleIdToken";
            var changePasswordDto = new ChangePasswordDto { /* fill in with appropriate data */ };

            _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, ErrorMessage = "sampleErrorMessage" });
            
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ChangePassword(idToken, changePasswordDto));
        }

        #endregion

        #region UpdateUser

        [Fact]
        public async Task UpdateUser_ValidTokenAndUserInfo_Success()
        {
            // Arrange
            var idToken = "validIdToken";
            var userInfoDto = new UserInfoDto { PhoneNumber = "1234567890" };
            var userId = Utilities.GetUserIdFromToken(idToken);
    
            // Act
            var result = await _service.UpdateUser(idToken, userInfoDto);

            // Assert
            Assert.Equal($"User {userId} Updated successfull", result);
        }

        [Fact]
        public async Task UpdateUser_ValidTokenAndUserInfo_Failure()
        {
            // Arrange
            var idToken = "validIdToken";
            var userInfoDto = new UserInfoDto { PhoneNumber = "1234567890" };
            _client.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, ErrorMessage = "Update failed" });

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateUser(idToken, userInfoDto));
        }

        [Fact]
        public async Task UpdateUser_InvalidToken_ExceptionThrown()
        {
            // Arrange
            var idToken = "invalidIdToken";
            var userInfoDto = new UserInfoDto { PhoneNumber = "1234567890" };

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateUser(idToken, userInfoDto));
        }

        [Fact]
        public async Task UpdateUser_InvalidUserInfo_ExceptionThrown()
        {
            // Arrange
            var idToken = "validIdToken";
            var userInfoDto = new UserInfoDto(); // missing PhoneNumber

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateUser(idToken, userInfoDto));
        }
        
        #endregion
        
}