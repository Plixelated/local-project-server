using Microsoft.AspNetCore.Identity;

namespace project_server.Dtos
{
    public class RegisterRequest : LoginRequest
    {
        public required string Email { get; set; }
    }
}
