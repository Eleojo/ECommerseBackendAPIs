using Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Core.AuthenticationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("customNameIdentifier", user.Id.ToString()), // Use a custom claim type for user ID
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Include the role // Add this line to include the role

            };
            //Console.WriteLine(user.Id.ToString());
            Console.WriteLine("Generated Claims: " + string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}")));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:7284", // Optional
                audience: "https://localhost:7284", // Optional
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool VerifyPassword(User user, string password)
        {
            var hasher = new PasswordHasher<User>();
            return hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;
        }
        public string HashPassword(string password)
        {
            var hasher = new PasswordHasher<User>();
            return hasher.HashPassword(new User(), password);
        }


    }
}
