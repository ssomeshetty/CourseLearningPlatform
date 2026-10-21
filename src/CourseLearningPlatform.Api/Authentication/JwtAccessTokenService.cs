using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace CourseLearningPlatform.Api.Authentication;

public sealed class JwtAccessTokenService(IConfiguration configuration) : IAccessTokenService
{
    public AccessToken Create(UserProfile user)
    {
        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured. Set it with user-secrets.");
        if (Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("JWT signing key must be at least 32 bytes long.");

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        var role = user.IsAdmin ? "Admin" : "Learner";
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(issuer, audience, claims, expires: expiresAtUtc, signingCredentials: credentials);
        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc, role);
    }
}
