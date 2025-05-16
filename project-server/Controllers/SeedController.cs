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
using System.Security.Cryptography.X509Certificates;

//This class handles seeding of data to the database

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

        //Restricts Test User seeding to User who can ManageUsers
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
            
            await userManager.AddToRoleAsync(user, "User");
            await context.SaveChangesAsync();
        }

        //Seeds Initial Admin Account
        //No restriction to allow for seeding when deployed
        //Password is handled by .env so no information is exposed
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
            //Load ENV
            DotNetEnv.Env.Load();
            //Create User with retrieved admin password
            IdentityResult results = await userManager.CreateAsync(user, Environment.GetEnvironmentVariable("AdminPassword"));
            
            //Checks if admin role exists, if not create it and assign to the seeded user
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                IdentityRole newRole = new IdentityRole("Admin");
                var addRole = await roleManager.CreateAsync(newRole);
                if (!addRole.Succeeded)
                    return BadRequest(addRole.Errors);
            }
            //Add Role to user
            var res = await userManager.AddToRoleAsync(user, "Admin");
            //Save Changes
            int save = await context.SaveChangesAsync();

            //If any issues happen, return error
            if (!results.Succeeded || !res.Succeeded)
                return BadRequest(results.Errors);

            return Ok("Admin account created succesfully!");

        }

        //------------------------------------------------------
        //The following endpoints are restricted via Admin Role
        //as Opposed to Policies. At this point, only the
        //Admin Role should exist. They are intended to only be
        //used on an empty database. This allows roles and claims
        //to be seeded when deployed while preventing non admin
        //accounts to seed authorization configurations.
        //-------------------------------------------------------

        //Allows seeding of Roles for Policies
        [Authorize(Roles = "Admin")]
        [HttpPost("roles")]
        public async Task<ActionResult> SeedRoles()
        {
            //Creates list of roles
            string[] roles = new[] { "Admin", "Researcher", "User" };
            
            foreach (var role in roles)
            {
                //If role doesn't exists
                if (!await roleManager.RoleExistsAsync(role))
                {
                    //Create new role
                    IdentityRole newRole = new IdentityRole(role);
                    //Add role to database
                    var res = await roleManager.CreateAsync(newRole);
                    //Return error if there are any issues
                    if (!res.Succeeded)
                        return BadRequest(res.Errors);
                }
            }

            return Ok("Roles Created");
        }

        //Seed All Claims for Admin Accounts
        [Authorize(Roles = "Admin")]
        [HttpPost("AdminClaims")]
        public async Task<ActionResult> SeedAdminClaims()
        {
            //If Admin role doesn't exist, send an error
            var role = await roleManager.FindByNameAsync("Admin");
            if (role == null)
                return NotFound("No Role Found");
            //Create list of Claims
            var claims = new List<Claim>
            {
                //Admin Account has all Permissions
                new Claim("Permission", "CanManageUsers"),
                new Claim("Permission", "CanViewUserData"),
                new Claim("Permission", "CanManageAllData"),
                new Claim("Permission", "HasUserID"),
            };

            //Gets existing claims
            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in claims)
            {
                //If the current claim doesn't exist on the role
                if (!existingClaims.Any(c => c.Value == claim.Value))
                {
                    //Add the claim
                    var res = await roleManager.AddClaimAsync(role, claim);
                    //Return error if there are any issues
                    if (!res.Succeeded)
                        return BadRequest(res.Errors);
                }
            }

            return Ok(new { message = "Claims Added to Admin Role" });
        }

        //Seed All Claims for Researcher Accounts
        [Authorize(Roles = "Admin")]
        [HttpPost("ResearcherClaims")]
        public async Task<ActionResult> SeedResearcherClaims()
        {
            //If researcher role doesn't exist, send an error
            var role = await roleManager.FindByNameAsync("Researcher");
            if (role == null)
                return NotFound("No Role Found");

            //Create list of Claims
            var claims = new List<Claim>
            {
                //Researcher Account Can manage and view user submissions
                new Claim("Permission", "CanManageAllData"),
                new Claim("Permission", "HasUserID"),
            };

            //Gets existing claims
            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in claims)
            {
                if (!existingClaims.Any(c => c.Value == claim.Value))
                {
                    //If the current claim doesn't exist on the role
                    var res = await roleManager.AddClaimAsync(role, claim);
                    //Return error if there are any issues
                    if (!res.Succeeded)
                        return BadRequest(res.Errors);
                }
            };

            return Ok(new { message = "Claims Added to Researcher Role" });
        }

        //Seed All Claims for User Accounts
        [Authorize(Roles = "Admin")]
        [HttpPost("UserClaims")]
        public async Task<ActionResult> SeedUserClaims()
        {
            //If User role doesn't exist, send an error
            var role = await roleManager.FindByNameAsync("User");
            if (role == null)
                return NotFound("No Role Found");

            //Create Claim, User Accounts can only view their own submissions
            var claim = new Claim("Permission", "HasUserID");
            //Gets existing claims
            var existingClaims = await roleManager.GetClaimsAsync(role);

            if (!existingClaims.Any(c => c.Value == claim.Value))
            {
                //If the current claim doesn't exist on the role
                var res = await roleManager.AddClaimAsync(role, claim);
                //Return error if there are any issues
                if (!res.Succeeded)
                    return BadRequest(res.Errors);
            }

            return Ok(new { message = "Claims Added to User Role" });
        }

        //-------------------------------------------------------
        //END ADMIN ONLY SEEDING
        //-------------------------------------------------------

        //Allows elevating an account to Admin Priveleges
        //Restricted to those who can ManageUsers
        [Authorize(Policy = "ManageUsers")]
        [HttpPost("assign-admin/{email}")]
        public async Task<ActionResult> AssignAdminRole(string email)
        {
            //Find User by Email
            var user = await userManager.FindByEmailAsync(email);
            //Return error if user not found
            if(user == null)
            {
                return NotFound("User not found");
            }
            //If current user isn't an admin
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                //Give them admin role
                var result = await userManager.AddToRoleAsync(user, "Admin");
                //If theres an issue return error
                if (!result.Succeeded)
                    return BadRequest(result.Errors);
            }

            return Ok($"User {email} elevated to Admin role.");
        }

        //Allows Seeding of Random Data Points
        //Restricted to those who can ManageAllData
        [Authorize(Policy = "ManageAllData")]
        [HttpPost("RandomData/{seedAmount}")]
        public async Task<ActionResult> SeedRandomData(int seedAmount)
        {
            //Track number of data points seeded
            int successes = 0;
            //Create new instace of random
            Random random = new Random();
            //Create new OriginID
            var originID = Guid.NewGuid().ToString();
            //Loop through Specified Amount
            for (int i = 0; i < seedAmount; i++)
            {
                //Every 10 submissions, create a new OriginID
                if (i % 10 == 0)
                {
                    originID = Guid.NewGuid().ToString();
                }
                //Seeds random values to the database
                var values =  new Dtos.ValuesDTO
                {
                    RateStars = (decimal)(random.NextDouble() * (3 - 0.1) + 0.1),
                    FrequencyPlanets = (decimal)((random.NextDouble() * (100 - 1) + 1) / 100),
                    NearEarth = (decimal)(random.NextDouble() * (5 - 0.1) + 0.1),
                    FractionLife = (decimal)((random.NextDouble() * (100 - 1) + 1) / 100),
                    FractionIntelligence = (decimal)((random.NextDouble() * (100 - 1) + 1) / 100),
                    FractionCommunication = (decimal)((random.NextDouble() * (100 - 1) + 1) / 100),
                    Length = (long)random.NextInt64(1, 10000000000),
                    EntryOrigin = originID,
                };

                //Check if entry exists with current origin
                var existingEntry = await context.Entries
                    .Include(e => e.SubmittedValues)
                    .FirstOrDefaultAsync(e => e.Origin == values.EntryOrigin);
                //If it doesn't
                if (existingEntry == null)
                {
                    //Create New Entry
                    var newEntry = _submissionSeedService.CreateEntry(values);
                    context.Entries.Add(newEntry);
                    try
                    {
                        await context.SaveChangesAsync();
                        successes++;
                    }
                    catch (DbUpdateException ex)
                    {
                        Console.WriteLine("Error Seeding Value:", ex);
                    }
                }
                else //If it does
                {
                    //Update Existing Entry
                    var newSubmission = _submissionSeedService.AddNewSubmission(values);
                    existingEntry?.SubmittedValues.Add(newSubmission);
                    try
                    {
                        await context.SaveChangesAsync();
                        successes++;
                    }
                    catch(DbUpdateException ex)
                    {
                        Console.WriteLine("Error Seeding Value:", ex);
                    }
                    
                }

                //Send SignalR Update
                await _dataService.UpdateData();

            }

            return Ok(new
            {
                message = successes + " values succsefully seeded."
            });
        }
    }
}
