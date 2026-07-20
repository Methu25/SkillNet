using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Repositories
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public SqlUserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string query = "SELECT * FROM Users WHERE Email = @Email";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public User? GetUserByEmail(string email)
        {
            const string query = "SELECT * FROM Users WHERE Email = @Email";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            const string query = "SELECT * FROM Users WHERE UserID = @UserID";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public User? GetUserById(int userId)
        {
            const string query = "SELECT * FROM Users WHERE UserID = @UserID";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        public bool EmailExists(string email)
        {
            const string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            var count = (int)(cmd.ExecuteScalar() ?? 0);
            return count > 0;
        }

        public async Task<bool> CreateUserAsync(User user, string passwordHash, string roleName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();
            try
            {
                // 1. Get Role ID
                const string roleQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                int roleId;
                using (var cmd = new SqlCommand(roleQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null) return false;
                    roleId = (int)result;
                }

                // 2. Insert User
                const string insertUserQuery = @"
                    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Status) 
                    OUTPUT INSERTED.UserID 
                    VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, 'Active')";

                int newUserId;
                using (var cmd = new SqlCommand(insertUserQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                    newUserId = (int)(await cmd.ExecuteScalarAsync())!;
                }

                // 3. Insert UserRole junction
                const string insertJunctionQuery = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)";
                using (var cmd = new SqlCommand(insertJunctionQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@UserID", newUserId);
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public bool CreateUser(User user, string passwordHash, string roleName)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                // 1. Get Role ID
                const string roleQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                int roleId;
                using (var cmd = new SqlCommand(roleQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    var result = cmd.ExecuteScalar();
                    if (result == null) return false;
                    roleId = (int)result;
                }

                // 2. Insert User
                const string insertUserQuery = @"
                    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Status) 
                    OUTPUT INSERTED.UserID 
                    VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, 'Active')";

                int newUserId;
                using (var cmd = new SqlCommand(insertUserQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                    newUserId = (int)cmd.ExecuteScalar()!;
                }

                // 3. Insert UserRole junction
                const string insertJunctionQuery = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)";
                using (var cmd = new SqlCommand(insertJunctionQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@UserID", newUserId);
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
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

        public List<string> GetUserRoles(int userId)
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

        public async Task IncrementFailedAttemptsAsync(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = "UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void IncrementFailedAttempts(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task ResetFailedAttemptsAsync(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = "UPDATE Users SET FailedLoginAttempts = 0, LockoutEnd = NULL, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void ResetFailedAttempts(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE Users SET FailedLoginAttempts = 0, LockoutEnd = NULL, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task LockAccountAsync(string email, DateTime lockoutEnd)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = "UPDATE Users SET LockoutEnd = @LockoutEnd, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@LockoutEnd", lockoutEnd);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void LockAccount(string email, DateTime lockoutEnd)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE Users SET LockoutEnd = @LockoutEnd, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@LockoutEnd", lockoutEnd);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task SetResetTokenAsync(string email, string token, DateTime expiry)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = "UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @Expiry, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            cmd.Parameters.AddWithValue("@Expiry", expiry);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void SetResetToken(string email, string token, DateTime expiry)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @Expiry, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            cmd.Parameters.AddWithValue("@Expiry", expiry);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task<User?> GetUserByResetTokenAsync(string token)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = "SELECT * FROM Users WHERE ResetToken = @ResetToken";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public User? GetUserByResetToken(string token)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "SELECT * FROM Users WHERE ResetToken = @ResetToken";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPasswordHash)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            const string query = "UPDATE Users SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, UpdatedAt = GETDATE() WHERE UserID = @UserID";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public bool ResetPassword(int userId, string newPasswordHash)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE Users SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, UpdatedAt = GETDATE() WHERE UserID = @UserID";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserID = (int)reader["UserID"],
                Email = reader["Email"].ToString()!,
                PasswordHash = reader["PasswordHash"].ToString()!,
                FirstName = reader["FirstName"].ToString()!,
                LastName = reader["LastName"].ToString()!,
                Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                Status = reader["Status"].ToString()!,
                FailedLoginAttempts = (int)reader["FailedLoginAttempts"],
                LockoutEnd = reader["LockoutEnd"] == DBNull.Value ? null : (DateTime)reader["LockoutEnd"],
                ResetToken = reader["ResetToken"] == DBNull.Value ? null : reader["ResetToken"].ToString(),
                ResetTokenExpiry = reader["ResetTokenExpiry"] == DBNull.Value ? null : (DateTime)reader["ResetTokenExpiry"],
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }
    }
}
