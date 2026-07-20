using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        User? GetUserByEmail(string email);

        Task<User?> GetUserByIdAsync(int userId);
        User? GetUserById(int userId);

        Task<bool> EmailExistsAsync(string email);
        bool EmailExists(string email);

        Task<bool> CreateUserAsync(User user, string passwordHash, string roleName);
        bool CreateUser(User user, string passwordHash, string roleName);

        Task<List<string>> GetUserRolesAsync(int userId);
        List<string> GetUserRoles(int userId);

        Task IncrementFailedAttemptsAsync(string email);
        void IncrementFailedAttempts(string email);

        Task ResetFailedAttemptsAsync(string email);
        void ResetFailedAttempts(string email);

        Task LockAccountAsync(string email, DateTime lockoutEnd);
        void LockAccount(string email, DateTime lockoutEnd);

        Task SetResetTokenAsync(string email, string token, DateTime expiry);
        void SetResetToken(string email, string token, DateTime expiry);

        Task<User?> GetUserByResetTokenAsync(string token);
        User? GetUserByResetToken(string token);

        Task<bool> ResetPasswordAsync(int userId, string newPasswordHash);
        bool ResetPassword(int userId, string newPasswordHash);
    }
}
