using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Repositories
{
    public class SqlRoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public SqlRoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        public async Task<int?> GetRoleIdByNameAsync(string roleName)
        {
            const string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@RoleName", roleName);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int?)result : null;
        }

        public int? GetRoleIdByName(string roleName)
        {
            const string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@RoleName", roleName);
            var result = cmd.ExecuteScalar();
            return result != null ? (int?)result : null;
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            const string query = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@RoleID", roleId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public bool AssignRoleToUser(int userId, int roleId)
        {
            const string query = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@RoleID", roleId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public async Task<List<string>> GetRolesByUserIdAsync(int userId)
        {
            var roles = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = @"
                SELECT r.RoleName 
                FROM Roles r 
                JOIN UserRole ur ON r.RoleID = ur.RoleID 
                WHERE ur.UserID = @UserID";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                roles.Add(reader["RoleName"].ToString()!);
            }
            return roles;
        }

        public List<string> GetRolesByUserId(int userId)
        {
            var roles = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = @"
                SELECT r.RoleName 
                FROM Roles r 
                JOIN UserRole ur ON r.RoleID = ur.RoleID 
                WHERE ur.UserID = @UserID";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                roles.Add(reader["RoleName"].ToString()!);
            }
            return roles;
        }
    }
}
