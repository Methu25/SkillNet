using Microsoft.Data.SqlClient;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Repositories
{
    public class SqlRoleRepository : IRoleRepository
    {
        private readonly AuthDbSession _session;

        public SqlRoleRepository(AuthDbSession session)
        {
            _session = session;
        }

        public async Task<int?> GetRoleIdByNameAsync(string roleName)
        {
            const string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@RoleName", roleName);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int?)result : null;
        }

        public int? GetRoleIdByName(string roleName)
        {
            const string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@RoleName", roleName);
            var result = cmd.ExecuteScalar();
            return result != null ? (int?)result : null;
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            const string query = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@RoleID", roleId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public bool AssignRoleToUser(int userId, int roleId)
        {
            const string query = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@RoleID", roleId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public async Task<List<string>> GetRolesByUserIdAsync(int userId)
        {
            var roles = new List<string>();
            const string query = @"
                SELECT r.RoleName 
                FROM Roles r 
                JOIN UserRole ur ON r.RoleID = ur.RoleID 
                WHERE ur.UserID = @UserID";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            const string query = @"
                SELECT r.RoleName 
                FROM Roles r 
                JOIN UserRole ur ON r.RoleID = ur.RoleID 
                WHERE ur.UserID = @UserID";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
