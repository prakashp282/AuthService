using System.Threading.Tasks;
using Auth.Models.Dtos;

namespace Auth.AuthenticationService.Services;

public interface IRoleManager
{
    Task<string> AddRole(UserRoleDto userRole);
    Task<string> RemoveRole(UserRoleDto userRole);
}