using System;
using System.Text.RegularExpressions;
using SkillNet.Server.Models;

namespace SkillNet.Server.Services
{
    public interface IAuthenticationService
    {
        string? ValidatePasswordPolicy(string password);
        bool Register(RegisterRequest request, out string errorMessage);
        AuthResponse? Login(LoginRequest request, out string errorMessage);
    }

    public class AuthenticationService(
        IUserService userService,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService) : IAuthenticationService
    {
        private readonly IUserService _userService = userService;
        private readonly IPasswordHashService _passwordHashService = passwordHashService;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

#pragma warning disable SYSLIB1045 // Use GeneratedRegexAttribute for source generator
        private static readonly Regex UpperCaseRegex = new("[A-Z]", RegexOptions.Compiled);
        private static readonly Regex LowerCaseRegex = new("[a-z]", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new("[0-9]", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new("[^a-zA-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex EmailFormatRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
#pragma warning restore SYSLIB1045

        public string? ValidatePasswordPolicy(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return "Password must be at least 8 characters long.";

            if (!UpperCaseRegex.IsMatch(password))
                return "Password must contain at least one uppercase letter.";

            if (!LowerCaseRegex.IsMatch(password))
                return "Password must contain at least one lowercase letter.";

            if (!NumberRegex.IsMatch(password))
                return "Password must contain at least one number.";

            if (!SpecialCharRegex.IsMatch(password))
                return "Password must contain at least one special character.";

            return null; // Valid
        }

        public bool Register(RegisterRequest request, out string errorMessage)
        {
            errorMessage = string.Empty;

            // 1. Email format check
            if (string.IsNullOrWhiteSpace(request.Email) || !EmailFormatRegex.IsMatch(request.Email))
            {
                errorMessage = "Invalid email format.";
                return false;
            }

            // 2. Email uniqueness check
            if (_userService.GetUserByEmail(request.Email) != null)
            {
                errorMessage = "Email is already registered.";
                return false;
            }

            // 3. Password policy check
            var policyError = ValidatePasswordPolicy(request.Password);
            if (policyError != null)
            {
                errorMessage = policyError;
                return false;
            }

            // 4. Hash and create
            string hash = _passwordHashService.HashPassword(request.Password);
            bool success = _userService.CreateUser(request, hash);
            if (!success)
            {
                errorMessage = "Failed to create user. Verify the specified role is valid.";
                return false;
            }

            return true;
        }

        public AuthResponse? Login(LoginRequest request, out string errorMessage)
        {
            errorMessage = string.Empty;

            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                errorMessage = "Invalid email or password.";
                return default;
            }

            // Check if user is locked out
            if (user.LockoutEnd > DateTime.UtcNow)
            {
                var timeLeft = user.LockoutEnd.Value - DateTime.UtcNow;
                errorMessage = $"Account locked. Try again in {Math.Ceiling(timeLeft.TotalMinutes)} minutes.";
                return default;
            }

            // Verify password
            bool isValid = _passwordHashService.VerifyPassword(request.Password, user.PasswordHash);
            if (!isValid)
            {
                // Increment failed attempts
                _userService.IncrementFailedAttempts(request.Email);

                // Reload user to check updated attempts
                var updatedUser = _userService.GetUserByEmail(request.Email)!;
                if (updatedUser.FailedLoginAttempts >= 5)
                {
                    var lockoutDuration = DateTime.UtcNow.AddMinutes(15);
                    _userService.LockAccount(request.Email, lockoutDuration);
                    errorMessage = "Account locked out for 15 minutes due to 5 failed attempts.";
                }
                else
                {
                    int attemptsLeft = 5 - updatedUser.FailedLoginAttempts;
                    errorMessage = $"Invalid credentials. {attemptsLeft} attempts remaining.";
                }
                return default;
            }

            // Reset failed login attempts on successful login
            _userService.ResetFailedAttempts(request.Email);

            // Fetch roles
            var roles = _userService.GetUserRoles(user.UserID);

            // Generate tokens
            string accessToken = _jwtTokenService.GenerateAccessToken(user.Email, roles);
            string refreshToken = _jwtTokenService.GenerateRefreshToken(user.UserID);

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                Roles = roles,
                Message = "Login successful"
            };
        }
    }
}
