using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using project_model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace project_server;

public class JWTHandler(IConfiguration configuration, UserManager<ProjectUser> userManager, RoleManager<IdentityRole> roleManager) //<- DI
{

    //Returns token to caller
    //Password in not stored in the token
    public async Task<JwtSecurityToken> GetTokenAsync(ProjectUser user) =>
        new(
            issuer: configuration["JWTSettings:Issuer"],
            audience: configuration["JWTSettings:Audience"],
            claims: await GetClaimsAsync(user),
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(configuration["JWTSettings:ExpirationTimeInMinutes"])),
            signingCredentials: GetSigningCredentials());

    //Username and Roles
    private SigningCredentials GetSigningCredentials()
    {
        byte[] key = Encoding.UTF8.GetBytes(configuration["JWTSettings_SecurityKey"]!);
        SymmetricSecurityKey secret = new(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private async Task<List<Claim>> GetClaimsAsync(ProjectUser user)
    {
        //Create list of claims assoiciated with the user
        List<Claim> claims = [new Claim(ClaimTypes.Name, user.UserName!)];
        
        //Add All Claims associated with the user's role to the JWT token
        claims.AddRange(await userManager.GetClaimsAsync(user));
        var roles = await userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            //Include Role
            claims.Add(new Claim(ClaimTypes.Role, roleName));
            var role = await roleManager.FindByNameAsync(roleName);
            //Include Claims
            var roleClaims = await roleManager.GetClaimsAsync(role);
            claims.AddRange(roleClaims);
        }

        return claims;
    }
}