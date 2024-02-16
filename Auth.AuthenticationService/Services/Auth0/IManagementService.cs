using System.Threading.Tasks;
using Auth0.ManagementApi;

namespace Auth.AuthenticationService.Services.Auth0
{
    public interface IManagementService
    {
        /// <summary>
        /// Gets an Active Managment api client.
        /// </summary>
        /// <returns></returns>
        Task<ManagementApiClient> GetManagmentApiClient();

        /// <summary>
        /// Checks if the provided Phone number is Unique or not.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        Task<bool> IsPhoneNumberUnique(string phoneNumber);
    }
}