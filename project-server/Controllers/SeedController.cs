using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_model;
using project_server.Dtos;
using System.Security.Claims;
using System.Data;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(
        ModelContext context, 
        UserManager<ProjectUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        SubmissionSeedService submissionSeedService,
        DataService dataService
        ) : ControllerBase
    {
        private readonly SubmissionSeedService _submissionSeedService = submissionSeedService;
        private readonly DataService _dataService = dataService;

        [Authorize(Policy = "ManageUsers")]
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
            
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                IdentityRole newRole = new IdentityRole("Admin");
                var addRole = await roleManager.CreateAsync(newRole);
                if (!addRole.Succeeded)
                    return BadRequest(addRole.Errors);
            }
            var res = await userManager.AddToRoleAsync(user, "Admin");
            int save = await context.SaveChangesAsync();

            //Add Claims to User for userid
            var newUser = await userManager.FindByEmailAsync(user.Email);
            if (newUser == null) return NotFound();

            //Adds User ID Claim
            var claim = new Claim("UserID", newUser.Id);
            var claimRes = await userManager.AddClaimAsync(newUser, claim);

            if (!results.Succeeded || !res.Succeeded || !claimRes.Succeeded)
                return BadRequest(results.Errors);

            return Ok("Admin account created succesfully!");

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("roles")]
        public async Task<ActionResult> SeedRoles()
        {
            string[] roles = new[] { "Admin", "Researcher", "User" };
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

        [Authorize(Roles = "Admin")]
        [HttpPost("AdminClaims")]
        public async Task<ActionResult> SeedAdminClaims()
        {
            var role = await roleManager.FindByNameAsync("Admin");
            if (role == null)
                return NotFound("No Role Found");

            var claims = new List<Claim>
            {
                new Claim("Permission", "CanManageUsers"),
                new Claim("Permission", "CanViewUserData"),
                new Claim("Permission", "CanManageData"),
            };

            foreach (var claim in claims)
            {
                var res = await roleManager.AddClaimAsync(role, claim);
                if (!res.Succeeded)
                    return BadRequest(res.Errors);
            }


            return Ok("Claims Added to Admin Role");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ResearcherClaims")]
        public async Task<ActionResult> SeedResearcherClaims()
        {
            var role = await roleManager.FindByNameAsync("Researcher");
            if (role == null)
                return NotFound("No Role Found");

            var claim = new Claim("Permission", "CanManageData");

            var res = await roleManager.AddClaimAsync(role, claim);
            if (!res.Succeeded)
                return BadRequest(res.Errors);

            return Ok("Claims Added to Admin Role");
        }


        [Authorize(Policy = "ManageUsers")]
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

        [Authorize(Policy = "ManageData")]
        [HttpPost("RandomData")]
        public async Task<ActionResult> SeedRandomData(int seedAmount)
        {
            //SubmissionController submissions = new(context);
            Random random = new Random();
            var originID = Guid.NewGuid().ToString();

            for (int i = 0; i < seedAmount; i++)
            {
                if (i % 10 == 0)
                {
                    originID = Guid.NewGuid().ToString();
                }

                var values =  new Dtos.ValuesDTO
                {
                    RateStars = (decimal)(random.NextDouble() * (3 - 0.1) + 0.1),
                    FrequencyPlanets = (decimal)(random.NextDouble() * (100 - 1) + 1),
                    NearEarth = (short)random.Next(1, 10),
                    FractionLife = (decimal)(random.NextDouble() * (100 - 1) + 1),
                    FractionIntelligence = (decimal)(random.NextDouble() * (100 - 1) + 1),
                    FractionCommunication = (decimal)(random.NextDouble() * (100 - 1) + 1),
                    Length = (long)random.NextInt64(1, 10000000000),
                    EntryOrigin = originID,
                };

                //INSERT ADDNEW SUBMISSION HERE
                //Check if entry exists with current origin
                var existingEntry = await context.Entries
                    .Include(e => e.SubmittedValues)
                    .FirstOrDefaultAsync(e => e.Origin == values.EntryOrigin);

                if (existingEntry == null)
                {
                    //Create New Entry
                    var newEntry = _submissionSeedService.CreateEntry(values);
                    context.Entries.Add(newEntry);
                    await context.SaveChangesAsync();
                }
                else
                {
                    //Update Existing Entry
                    var newSubmission = _submissionSeedService.AddNewSubmission(values);
                    existingEntry?.SubmittedValues.Add(newSubmission);
                    await context.SaveChangesAsync();
                }

                //Send SignalR Update
                await _dataService.UpdateData();

            }

            return Ok(new
            {
                message = "Data Seeded."
            });
        }
    }
}
