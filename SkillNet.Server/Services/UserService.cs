using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Server.Models;

namespace SkillNet.Server.Services
{
    public interface IUserService
    {
        User? GetUserByEmail(string email);
        User? GetUserById(int userId);
        bool CreateUser(RegisterRequest request, string passwordHash);
        List<string> GetUserRoles(int userId);
        void IncrementFailedAttempts(string email);
        void ResetFailedAttempts(string email);
        void LockAccount(string email, DateTime lockoutEnd);
        void SetResetToken(string email, string token, DateTime expiry);
        User? GetUserByResetToken(string token);
        bool ResetPassword(int userId, string newPasswordHash);
    }

    public class UserService(IConfiguration config) : IUserService
    {
        private readonly string _connectionString = config.GetConnectionString("DefaultConnection")!;

        public User? GetUserByEmail(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "SELECT * FROM Users WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        public User? GetUserById(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "SELECT * FROM Users WHERE UserID = @UserID";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        public bool CreateUser(RegisterRequest request, string passwordHash)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                // 1. Find Role ID
                const string roleQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                int roleId = 0;
                using (var cmd = new SqlCommand(roleQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@RoleName", request.RoleName);
                    var result = cmd.ExecuteScalar();
                    if (result == null) return false;
                    roleId = (int)result;
                }

                // 2. Insert User
                const string insertUserQuery = @"
                    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Status) 
                    OUTPUT INSERTED.UserID 
                    VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, 'Active')";

                int newUserId = 0;
                using (var cmd = new SqlCommand(insertUserQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", request.LastName);
                    cmd.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
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

        public void IncrementFailedAttempts(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            const string query = "UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1, UpdatedAt = GETDATE() WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
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
