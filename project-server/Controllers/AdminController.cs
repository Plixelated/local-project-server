using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(
        UserManager<ProjectUser> userManager, 
        JWTHandler jwtHandler, 
        RoleManager<IdentityRole> roleManager,
        ModelContext context
        ) : ControllerBase
    {
        private ModelContext _context = context;


        [HttpGet("Test")]
        public IActionResult Test() => Ok("Test passed");

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
                SameSite = SameSiteMode.None,
                Secure = true
            };

            Response.Cookies.Append("jwt", tokenString, cookieOptions);
            //CURRENTLY CAUSES A 400 ERROR ON LOGIN
/*            var refreshToken = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { Message = "Invalid Token" });*/

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login Succesful",
                Token = tokenString
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

            //Add Claims to User for userid
            var newUser = await userManager.FindByEmailAsync(request.Email);
            if (newUser == null) return NotFound();

            //Redundant since it is added to roles now
            //Adds User ID Claim
/*            var claim = new Claim("UserID", newUser.Id);
            var res = await userManager.AddClaimAsync(newUser, claim);
            if (!res.Succeeded)
                return BadRequest(res.Errors);*/


            //JANK SHIT HERE
            //TODO: Remove UserOrigin Table, change Entry Table to origin table
            //Add column for optional UserID as a FK to userID table with a 1:1 relationship
            //Cleaner more effective, and avoids this jank below
            //Check if entry exists with current origin
            var existingEntry = await _context.Entries
                .Include(e => e.SubmittedValues)
                .FirstOrDefaultAsync(e => e.Origin == request.OriginID);

            if (existingEntry == null)
            {
                var newEntry = new Entry
                {
                    Origin = request.OriginID,
                    SubmittedValues = new List<Values>()
                };
                _context.Entries.Add(newEntry);
            }
            //END JANKY SHIT

            UserOrigin originUserLink = new()
            {
                UserId = newUser.Id,
                EntryOrigin = request.OriginID
            };

            _context.UserOrigin.Add(originUserLink);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = $"User {request.UserName} created succesfully."
            });
        }

        [Authorize(Policy ="ManageUsers")]
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

            return Ok(new
            {
                Success = true,
                Message = $"Admin {request.UserName} created succesfully.",
            });
        }

        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [Authorize(Policy = "ManageAllData")]
        [HttpGet("AdminOnlyTest")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("This is an admin-only endpoint.");
        }

        [HttpGet("GetUserRole")]
        public IActionResult GetUserRole()
        {
            return Ok(User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => new { c.Value }).ToList());
        }


        [HttpPost("SetUserOriginID/{originID}")]
        public async Task<IActionResult> SetUserOriginID(string originID)
        {
            //Get User ID
            var userName = User?.Identity?.Name;
            var userInfo = await userManager.FindByNameAsync(userName);
            var userID = userInfo!.Id;

            if (userID != string.Empty)
            {

                //Check if Link Already Exists
                var link = await _context.UserOrigin
                                .FirstOrDefaultAsync(e => e.UserId == userID);

                //If Link exists
                if (link != null)
                {
                    return Ok("User has existing origin.");
                }

                //If link doesn't exist
                UserOrigin originUserLink = new()
                {
                    UserId = userID,
                    EntryOrigin = originID
                };

                _context.UserOrigin.Add(originUserLink);
                await _context.SaveChangesAsync();

                return Ok("User Origin Link Added");
            }

            return BadRequest("User Not Found.");

        }

        
        [HttpGet("GetUserOriginID")]
        public async Task<ActionResult> GetUserOriginID()
        {
            var userName = User?.Identity?.Name;
            var userInfo = await userManager.FindByNameAsync(userName);
            var userID = userInfo!.Id;

            var link = await _context.UserOrigin.FirstOrDefaultAsync(e => e.UserId == userID);

            if (link != null)
                return Ok(new { link?.EntryOrigin });

            return BadRequest("No Origin ID found");
        }

    }

}
