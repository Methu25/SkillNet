using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Repositories
{
    public class SqlRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public SqlRefreshTokenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        public async Task CreateRefreshTokenAsync(int userId, string token, DateTime expiresAt)
        {
            const string query = "INSERT INTO RefreshTokens (UserID, Token, ExpiresAt) VALUES (@UserID, @Token, @ExpiresAt)";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Token", token);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            await cmd.ExecuteNonQueryAsync();
        }

        public void CreateRefreshToken(int userId, string token, DateTime expiresAt)
        {
            const string query = "INSERT INTO RefreshTokens (UserID, Token, ExpiresAt) VALUES (@UserID, @Token, @ExpiresAt)";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Token", token);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            cmd.ExecuteNonQuery();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            const string query = "SELECT TokenID, UserID, Token, ExpiresAt, IsRevoked, CreatedAt FROM RefreshTokens WHERE Token = @Token";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", token);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapRefreshToken(reader);
            }
            return null;
        }

        public RefreshToken? GetRefreshToken(string token)
        {
            const string query = "SELECT TokenID, UserID, Token, ExpiresAt, IsRevoked, CreatedAt FROM RefreshTokens WHERE Token = @Token";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", token);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapRefreshToken(reader);
            }
            return null;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            const string query = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", token);
            await cmd.ExecuteNonQueryAsync();
        }

        public void RevokeRefreshToken(string token)
        {
            const string query = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", token);
            cmd.ExecuteNonQuery();
        }

        private static RefreshToken MapRefreshToken(SqlDataReader reader)
        {
            return new RefreshToken
            {
                TokenID = (int)reader["TokenID"],
                UserID = (int)reader["UserID"],
                Token = reader["Token"].ToString()!,
                ExpiresAt = (DateTime)reader["ExpiresAt"],
                IsRevoked = (bool)reader["IsRevoked"],
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }
    }
}
