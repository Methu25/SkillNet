using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SkillNet.Application.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(string email, List<string> roles);
        string GenerateRefreshToken(int userId);
        int? ValidateRefreshToken(string refreshToken);
        void RevokeRefreshToken(string refreshToken);
    }

    public class JwtTokenService(IConfiguration config) : IJwtTokenService
    {
        private readonly IConfiguration _config = config;
        private readonly string _connectionString = config.GetConnectionString("DefaultConnection")!;

        public string GenerateAccessToken(string email, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, email),
                new(JwtRegisteredClaimNames.Sub, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(int userId)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            string tokenString = Convert.ToBase64String(randomNumber);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "INSERT INTO RefreshTokens (UserID, Token, ExpiresAt) VALUES (@UserID, @Token, @ExpiresAt)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Token", tokenString);
            cmd.Parameters.AddWithValue("@ExpiresAt", DateTime.UtcNow.AddDays(7));
            cmd.ExecuteNonQuery();

            return tokenString;
        }

        public int? ValidateRefreshToken(string refreshToken)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "SELECT UserID, ExpiresAt, IsRevoked FROM RefreshTokens WHERE Token = @Token";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", refreshToken);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int userId = (int)reader["UserID"];
                DateTime expiresAt = (DateTime)reader["ExpiresAt"];
                bool isRevoked = (bool)reader["IsRevoked"];

                if (expiresAt > DateTime.UtcNow && !isRevoked)
                {
                    return userId;
                }
            }
            return null;
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", refreshToken);
            cmd.ExecuteNonQuery();
        }
    }
}
