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

            //HTTP ONLY COOKIE
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = token.ValidTo,
                SameSite = SameSiteMode.Strict,
                Secure = true
            };

            Response.Cookies.Append("jwt", tokenString, cookieOptions);
            var refreshToken = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { Message = "Invalid Token" });

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login Succesful",
            });
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged Out" });
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

        [HttpPost("RegisterAdmin")]
        public async Task<ActionResult> RegisterAdminAsync(Dtos.RegisterRequest request)
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

            bool userRole = await roleManager.RoleExistsAsync("Admin");
            if (!userRole)
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            await userManager.AddToRoleAsync(user, "Admin");

            return Ok($"Admin {request.UserName} created succesfully.");
        }
    }
}
