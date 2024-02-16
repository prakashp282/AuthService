namespace Auth.Models.Dtos;

public class UserRoleDto
{
    public UserRoleDto(string email, string role)
    {
        Email = email;
        Role = role;
    }

    public string Email { get; set; }
    public string Role { get; set; }
}