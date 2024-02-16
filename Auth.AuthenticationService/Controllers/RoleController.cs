using System.Threading.Tasks;
using Auth.AuthenticationService.Services;
using Auth.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.AuthenticationService.Controllers;

[Route("role")]
[ApiController]
public class RoleController : ControllerBase
{
    private IRoleManager _roleManager;

    public RoleController(IRoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpPost]
    [Route("add")]
    [Authorize]
    public async Task<IActionResult> AddRole(UserRoleDto userRole)
    {
        ApiResponseDto apiResponse = new ApiResponseDto(message: await _roleManager.AddRole(userRole));
        return Ok(apiResponse);
    }

    [HttpPost("remove")]
    [Authorize]
    public async Task<IActionResult> RemoveRole(UserRoleDto userRole)
    {
        ApiResponseDto apiResponse = new ApiResponseDto(message: await _roleManager.RemoveRole(userRole));
        return Ok(apiResponse);
    }
}