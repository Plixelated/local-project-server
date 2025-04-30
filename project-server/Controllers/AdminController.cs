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
    public class AdminController(
        UserManager<ProjectUser> userManager, JWTHandler jwtHandler, RoleManager<IdentityRole> roleManager
        ) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> LoginAsync(Dtos.LoginRequest request)
        {
            ProjectUser user = await userManager.FindByNameAsync(request.UserName);

            if (user == null)
                return Unauthorized("Incorrect Login Information");

            bool success = await userManager.CheckPasswordAsync(user, request.Password);

            if (!success)
                return Unauthorized("Incorrect Login Information");

            JwtSecurityToken token = await jwtHandler.GetTokenAsync(user);
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login Succesful",
                Token = tokenString,
            });
        }

        [HttpPost("Register")]
        public async Task<ActionResult> RegisterAsync(Dtos.RegisterRequest request)
        {
            ProjectUser user = new()
            {
                UserName = request.UserName,
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            IdentityResult userResults = await userManager.CreateAsync(user, request.Password);

            if (!userResults.Succeeded)
                return BadRequest(userResults.Errors);

            bool userRole = await roleManager.RoleExistsAsync("User");
            if (!userRole)
                await roleManager.CreateAsync(new IdentityRole("User"));

            await userManager.AddToRoleAsync(user, "User");

            return Ok($"User {request.UserName} created succesfully.");
        }
    }
}
