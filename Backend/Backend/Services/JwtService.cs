using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.dto;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Issuer => _configuration["Jwt:Issuer"] ?? "VietnameseLearningApp";

        public string Audience => _configuration["Jwt:Audience"] ?? "VietnameseLearningClient";

        public int AccessTokenLifetimeMinutes =>
            int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

        public string GenerateAccessToken(UserDTO user, string roleName)
        {
            var secretKey = _configuration["AppSettings:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Missing AppSettings:SecretKey configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: authClaims,
                expires: DateTime.UtcNow.AddMinutes(AccessTokenLifetimeMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>Backward-compatible name used by older code paths.</summary>
        public string GenerateToken(UserDTO user, string roleName) => GenerateAccessToken(user, roleName);
    }
}
