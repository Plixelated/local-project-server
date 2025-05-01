using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using project_model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace project_server;

public class JWTHandler(IConfiguration configuration, UserManager<ProjectUser> userManager) //<- DI
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
        List<Claim> claims = [new Claim(ClaimTypes.Name, user.UserName!)];
        claims.AddRange(from role in await userManager.GetRolesAsync(user) select new Claim(ClaimTypes.Role, role));
        return claims;
    }
}