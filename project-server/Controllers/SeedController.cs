using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_model;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(ModelContext context, IHostEnvironment environment, UserManager<ProjectUser> userManager) : ControllerBase
    {
        [HttpPost("Users")]
        public async Task ImportUsersAsync()
        {
            //USER
            ProjectUser user = new()
            {
                UserName = "testUser",
                Email = "testUser@email.com",
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            //UserManager class is how you handle user related operations
            //password requires uppercase, digit, and special char
            IdentityResult results = await userManager.CreateAsync(user, "P4ssw0rd!");

            int save = await context.SaveChangesAsync();
        }
    }
}
