using SkillNet.Domain.Entities;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
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

    public class UserService(IUserRepository userRepository, IRoleRepository roleRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRoleRepository _roleRepository = roleRepository;

        public User? GetUserByEmail(string email)
        {
            return _userRepository.GetUserByEmail(email);
        }

        public User? GetUserById(int userId)
        {
            return _userRepository.GetUserById(userId);
        }

        public bool CreateUser(RegisterRequest request, string passwordHash)
        {
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Status = "Active"
            };

            return _userRepository.CreateUser(user, passwordHash, request.RoleName);
        }

        public List<string> GetUserRoles(int userId)
        {
            return _userRepository.GetUserRoles(userId);
        }

        public void IncrementFailedAttempts(string email)
        {
            _userRepository.IncrementFailedAttempts(email);
        }

        public void ResetFailedAttempts(string email)
        {
            _userRepository.ResetFailedAttempts(email);
        }

        public void LockAccount(string email, DateTime lockoutEnd)
        {
            _userRepository.LockAccount(email, lockoutEnd);
        }

        public void SetResetToken(string email, string token, DateTime expiry)
        {
            _userRepository.SetResetToken(email, token, expiry);
        }

        public User? GetUserByResetToken(string token)
        {
            return _userRepository.GetUserByResetToken(token);
        }

        public bool ResetPassword(int userId, string newPasswordHash)
        {
            return _userRepository.ResetPassword(userId, newPasswordHash);
        }
    }
}
