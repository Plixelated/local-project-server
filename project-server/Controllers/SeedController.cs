using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_model;
using project_server.Dtos;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(
        ModelContext context, UserManager<ProjectUser> userManager, RoleManager<IdentityRole> roleManager
        ) : ControllerBase
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
            DotNetEnv.Env.Load();
            IdentityResult results = await userManager.CreateAsync(user, Environment.GetEnvironmentVariable("TestPassword"));
            var res = await userManager.AddToRoleAsync(user, "User");
            int save = await context.SaveChangesAsync();
        }

        [HttpPost("Admin")]
        public async Task<ActionResult> ImportAdminAsync()
        {

            //USER
            ProjectUser user = new()
            {
                UserName = "admin",
                Email = "admin@plixelated.mozmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            //UserManager class is how you handle user related operations
            //password requires uppercase, digit, and special char
            DotNetEnv.Env.Load();
            IdentityResult results = await userManager.CreateAsync(user, Environment.GetEnvironmentVariable("AdminPassword"));
            var res = await userManager.AddToRoleAsync(user, "Admin");
            int save = await context.SaveChangesAsync();

            if(!results.Succeeded || !res.Succeeded)
                return BadRequest(results.Errors);

            return Ok("Admin account created succesfully!");

        }

        [HttpPost("roles")]
        public async Task<ActionResult> SeedRoles()
        {
            string[] roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    IdentityRole newRole = new IdentityRole(role);
                    var res = await roleManager.CreateAsync(newRole);
                    if (!res.Succeeded)
                        return BadRequest(res.Errors);
                }
            }

            return Ok("Roles Created");
        }

        [HttpPost("assign-admin/{email}")]
        public async Task<ActionResult> AssignAdminRole(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return NotFound("User not found");
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await userManager.AddToRoleAsync(user, "Admin");
                
                if (!result.Succeeded)
                    return BadRequest(result.Errors);
            }

            return Ok($"User {email} elevated to Admin role.");
        }

        [HttpPost("RandomData")]
        public async Task<ActionResult> SeedRandomData()
        {
            for (int i = 0; i < 100; i++) {
                Random random = new Random();
                var newEntry = new Entry
                {
                    Origin = Guid.NewGuid().ToString(),
                    SubmittedValues = new List<Values>
                {
                    new Values
                    {
                        RateStars = (decimal)(random.NextDouble() * (3-0.1) + 0.1),
                        FrequencyPlanets = (decimal)(random.NextDouble() * (100-1) + 1),
                        NearEarth = (short)random.Next(1,10),
                        FractionLife = (decimal)(random.NextDouble() * (100-1) + 1),
                        FractionIntelligence = (decimal)(random.NextDouble() * (100-1) + 1),
                        FractionCommunication = (decimal)(random.NextDouble() * (100-1) + 1),
                        Length = (long)random.NextInt64(1,10000000000),
                    }
                }
                };

                context.Entries.Add(newEntry);
                await context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Data Seeded."
            });
        }
    }
}
