using Microsoft.Data.SqlClient;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Repositories
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly AuthDbSession _session;

        public SqlUserRepository(AuthDbSession session)
        {
            _session = session;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string query = "SELECT * FROM Users WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", email);
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        public bool EmailExists(string email)
        {
            const string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", email);
            var count = (int)(cmd.ExecuteScalar() ?? 0);
            return count > 0;
        }

        public async Task<bool> CreateUserAsync(User user, string passwordHash)
        {
            const string insertUserQuery = @"
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Status) 
                OUTPUT INSERTED.UserID 
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, 'Active')";

            using var cmd = new SqlCommand(insertUserQuery, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null)
            {
                user.UserID = (int)result;
                return true;
            }
            return false;
        }

        public bool CreateUser(User user, string passwordHash)
        {
            const string insertUserQuery = @"
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Status) 
                OUTPUT INSERTED.UserID 
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, 'Active')";

            using var cmd = new SqlCommand(insertUserQuery, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                user.UserID = (int)result;
                return true;
            }
            return false;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
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

        public List<string> GetUserRoles(int userId)
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

        public async Task IncrementFailedAttemptsAsync(string email)
        {
            const string query = "UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void IncrementFailedAttempts(string email)
        {
            const string query = "UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task ResetFailedAttemptsAsync(string email)
        {
            const string query = "UPDATE Users SET FailedLoginAttempts = 0, LockoutEnd = NULL, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void ResetFailedAttempts(string email)
        {
            const string query = "UPDATE Users SET FailedLoginAttempts = 0, LockoutEnd = NULL, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task LockAccountAsync(string email, DateTime lockoutEnd)
        {
            const string query = "UPDATE Users SET LockoutEnd = @LockoutEnd, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@LockoutEnd", lockoutEnd);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void LockAccount(string email, DateTime lockoutEnd)
        {
            const string query = "UPDATE Users SET LockoutEnd = @LockoutEnd, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@LockoutEnd", lockoutEnd);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task SetResetTokenAsync(string email, string token, DateTime expiry)
        {
            const string query = "UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @Expiry, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            cmd.Parameters.AddWithValue("@Expiry", expiry);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public void SetResetToken(string email, string token, DateTime expiry)
        {
            const string query = "UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @Expiry, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@ResetToken", token);
            cmd.Parameters.AddWithValue("@Expiry", expiry);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public async Task<User?> GetUserByResetTokenAsync(string token)
        {
            const string query = "SELECT * FROM Users WHERE ResetToken = @ResetToken";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            const string query = "SELECT * FROM Users WHERE ResetToken = @ResetToken";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
            const string query = "UPDATE Users SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, UpdatedAt = GETDATE() WHERE UserID = @UserID";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public bool ResetPassword(int userId, string newPasswordHash)
        {
            const string query = "UPDATE Users SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, UpdatedAt = GETDATE() WHERE UserID = @UserID";
            using var cmd = new SqlCommand(query, _session.Connection, _session.Transaction);
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
