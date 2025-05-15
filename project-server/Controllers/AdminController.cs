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

//This controller handles all of the api requests regarding
//everything to do with user accounts.

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

        //Test API to ensure connection when deployed
        [HttpGet("Test")]
        public IActionResult Test() => Ok("Test passed");


        [HttpPost("Login")]
        public async Task<ActionResult> LoginAsync(Dtos.LoginRequest request)
        {
            //Checks if user name is correct
            ProjectUser user = await userManager.FindByNameAsync(request.UserName);
            //If not return an error message
            if (user == null)
                return Unauthorized("Incorrect Login Information");

            //Checks if password is correct
            bool success = await userManager.CheckPasswordAsync(user, request.Password);
            //If not return an error message
            if (!success)
                return Unauthorized("Incorrect Login Information");

            //Create JWT Token user JWT handler
            JwtSecurityToken token = await jwtHandler.GetTokenAsync(user);
            //Create the token string
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //Opted for a hybrid solution for JWT token handling on the frontend
            //Utilizes local storage and HTTPOnly Cookie

            //For HTTP ONLY COOKIE
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = token.ValidTo,
                SameSite = SameSiteMode.None,
                Secure = true
            };

            //Add JWT token to the cookie
            Response.Cookies.Append("jwt", tokenString, cookieOptions);

            //Return login request with token
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
            //Delete the cookie on logout
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged Out" });
        }

        //TODO: Add feedback for malformed password
        //Add Feedback for username already exists
        //Add solution for originID already exists
        [HttpPost("Register")]
        public async Task<ActionResult> RegisterAsync(Dtos.RegisterRequest request)
        {
            //Create new ProjectUser
            ProjectUser user = new()
            {
                UserName = request.UserName,
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            //Add new user to database
            IdentityResult userResults = await userManager.CreateAsync(user, request.Password);
            //If there is an issue, return error message
            if (!userResults.Succeeded)
                return BadRequest(userResults.Errors);

            //Ensures User role exists already, if not create the role
            bool userRole = await roleManager.RoleExistsAsync("User");
            if (!userRole)
                await roleManager.CreateAsync(new IdentityRole("User"));

            //Add User role to each user by default
            await userManager.AddToRoleAsync(user, "User");

            //Find the newly registered user
            var newUser = await userManager.FindByEmailAsync(request.Email);
            if (newUser == null) return NotFound();

            //--------------------------------------------
            //This ensures that a newly registered user with
            //no existing entry can make submissions
            //-----------------------------------------------
            //Check if entry exists with current origin
            var existingEntry = await _context.Entries
                .Include(e => e.SubmittedValues)
                .FirstOrDefaultAsync(e => e.Origin == request.OriginID);
            //If it doesn't
            if (existingEntry == null)
            {
                //Generate a blank entry
                var newEntry = new Entry
                {
                    Origin = request.OriginID,
                    SubmittedValues = new List<Values>()
                };
                _context.Entries.Add(newEntry);
            }

            //TODO: Remove UserOrigin Table, change Entry Table to origin table
            //Add column for optional UserID as a FK to userID table with a 1:1 relationship

            //Create new UserID OriginID link
            UserOrigin originUserLink = new()
            {
                UserId = newUser.Id,
                EntryOrigin = request.OriginID
            };
            //Add the link
            _context.UserOrigin.Add(originUserLink);
            //Save the changes to the database
            await _context.SaveChangesAsync();
            //Send message informing user was registered
            return Ok(new
            {
                Success = true,
                Message = $"User {request.UserName} created succesfully."
            });
        }

        //Policy ensures only those who can Manage Users can register a new admin user
        [Authorize(Policy ="ManageUsers")]
        [HttpPost("RegisterAdmin")]
        //Same as standard register, but registers user as an admin
        //Endpoint exists and referenced in the frontend, but no page
        //exists yet to allow for this
        public async Task<ActionResult> RegisterAdminAsync(Dtos.RegisterRequest request)
        {
            //Create new ProjectUser
            ProjectUser user = new()
            {
                UserName = request.UserName,
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            //Add new user to database
            IdentityResult userResults = await userManager.CreateAsync(user, request.Password);

            //If there is an issue, return error message
            if (!userResults.Succeeded)
                return BadRequest(userResults.Errors);

            //Ensures Admin role exists already, if not create the role
            bool userRole = await roleManager.RoleExistsAsync("Admin");
            if (!userRole)
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            //Add Admin role to this account
            await userManager.AddToRoleAsync(user, "Admin");

            //Find the newly registered user
            var newUser = await userManager.FindByEmailAsync(request.Email);
            if (newUser == null) return NotFound();

            //--------------------------------------------
            //This ensures that a newly registered user with
            //no existing entry can make submissions
            //-----------------------------------------------
            var existingEntry = await _context.Entries
                .Include(e => e.SubmittedValues)
                .FirstOrDefaultAsync(e => e.Origin == request.OriginID);
            //If it doesn't
            if (existingEntry == null)
            {
                //Generate a blank entry
                var newEntry = new Entry
                {
                    Origin = request.OriginID,
                    SubmittedValues = new List<Values>()
                };
                _context.Entries.Add(newEntry);
            }

            //TODO: Remove UserOrigin Table, change Entry Table to origin table
            //Add column for optional UserID as a FK to userID table with a 1:1 relationship

            //Create new UserID OriginID link
            UserOrigin originUserLink = new()
            {
                UserId = newUser.Id,
                EntryOrigin = request.OriginID
            };
            //Add the link
            _context.UserOrigin.Add(originUserLink);
            //Save the changes to the database
            await _context.SaveChangesAsync();
            //Send message informing user was registered
            return Ok(new
            {
                Success = true,
                Message = $"Admin {request.UserName} created succesfully.",
            });
        }

        //Get Current User's Claims for testing purposes
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        //Tests ManageAllData Policy
        [Authorize(Policy = "ManageAllData")]
        [HttpGet("AdminOnlyTest")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("This is an admin-only endpoint.");
        }

        //Get Current Users Role
        [HttpGet("GetUserRole")]
        public IActionResult GetUserRole()
        {
            return Ok(User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => new { c.Value }).ToList());
        }

        //This manually links an OriginID with a UserID
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
                //Addl link to the Database
                _context.UserOrigin.Add(originUserLink);
                await _context.SaveChangesAsync();

                return Ok("User Origin Link Added");
            }

            return BadRequest("User Not Found.");

        }

        //Get current users OriginID
        [HttpGet("GetUserOriginID")]
        public async Task<ActionResult> GetUserOriginID()
        {
            //Find User
            var userName = User?.Identity?.Name;
            //Get their User info
            var userInfo = await userManager.FindByNameAsync(userName);
            //Extract User ID
            var userID = userInfo!.Id;
            //Find their OriginID
            var link = await _context.UserOrigin.FirstOrDefaultAsync(e => e.UserId == userID);
            //Return OriginID
            if (link != null)
                return Ok(new { link?.EntryOrigin });
            //If not found, return error
            return BadRequest("No Origin ID found");
        }

    }

}
