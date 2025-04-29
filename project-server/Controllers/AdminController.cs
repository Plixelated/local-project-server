using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using project_model;
using project_server.Dtos;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<ProjectUser> userManager, JWTHandler jwtHandler) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> LoginAsync(Dtos.LoginRequest request)
        {
            ProjectUser user = await userManager.FindByNameAsync(request.UserName);

            if (user == null)
                return Unauthorized("Who even are you?");

            bool success = await userManager.CheckPasswordAsync(user, request.Password);

            if (!success)
                return Unauthorized("You don't know your password?");

            JwtSecurityToken token = await jwtHandler.GetTokenAsync(user);
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Other Mother loves you too",
                Token = tokenString,
            });
        }
    }
}
