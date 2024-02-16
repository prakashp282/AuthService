using System;
using System.Threading.Tasks;
using Auth.Models.Dtos;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Auth.AuthenticationService.Services.IdentityServer;

public class RoleService : IRoleManager
{
    private readonly IRestClient _client;
    private readonly ILogger<RoleService> _logger;

    public RoleService(ILogger<RoleService> logger, IRestClient client = null)
    {
        _client = client ?? new RestClient(Globals.IdentityServerHost);
        _logger = logger;
    }

    public async Task<string> AddRole(UserRoleDto userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);
        _logger.LogInformation("Adding role to User with Identity Server");
        var request = new RestRequest(AddRoleUrl, Method.Post);
        request.AddBody(userRole);
        var response = await _client.ExecuteAsync(request);
        if (response is not { IsSuccessful: true })
        {
            _logger.LogError("Error while Adding role ", response.Content);
            // Handle registration error
            throw new Exception("Error Adding role to User with Auth.IdentityServer: " + response.Content);
        }

        return "Role Added!";
    }

    public async Task<string> RemoveRole(UserRoleDto userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);
        _logger.LogInformation("Removing role to User with Identity Server");
        var request = new RestRequest(RemoveRoleUrl, Method.Post);
        request.AddBody(userRole);
        var response = await _client.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            _logger.LogError("Error while Removing role ", response.Content);
            // Handle registration error
            throw new Exception("Error Removing role to User with Auth.IdentityServer: " + response.Content);
        }

        return "Role Removed!";
    }

    #region ExposedEndPoints

    private const string AddRoleUrl = "/AddRole";
    private const string RemoveRoleUrl = "/RemoveRole";

    #endregion
}