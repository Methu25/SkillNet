using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.Services;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthenticationService authService,
        IUserService userService,
        IJwtTokenService jwtTokenService,
        IPasswordHashService passwordHashService,
        IEmailService emailService,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthenticationService _authService = authService;
        private readonly IUserService _userService = userService;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
        private readonly IPasswordHashService _passwordHashService = passwordHashService;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!_authService.Register(request, out string error))
                return BadRequest(new { Message = error });

            return Ok(new { Message = "Registration successful!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { Message = "Email and password are required." });

                var response = _authService.Login(request, out string errorMessage);

                if (response == null)
                    return Unauthorized(new { Message = errorMessage ?? "Invalid email or password." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Exception: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error. Please try again later." });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { Message = "Refresh token required." });

            _jwtTokenService.RevokeRefreshToken(request.RefreshToken);
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { Message = "Refresh token required." });

            var userId = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
            if (!userId.HasValue)
                return Unauthorized(new { Message = "Invalid or expired refresh token." });

            var user = _userService.GetUserById(userId.Value);
            if (user == null || user.Status != "Active")
                return Unauthorized(new { Message = "User inactive or not found." });

            _jwtTokenService.RevokeRefreshToken(request.RefreshToken);

            var roles = _userService.GetUserRoles(user.UserID);
            string newAccessToken = _jwtTokenService.GenerateAccessToken(user.Email, roles);
            string newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.UserID);

            return Ok(new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Email = user.Email,
                Roles = roles,
                Message = "Token refreshed successfully"
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            const string responseMessage = "If the email is registered, a password reset token has been sent.";

            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { Message = "Email is required." });

            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
                return Ok(new { Message = responseMessage });

            string resetToken = Guid.NewGuid().ToString("N");
            DateTime expiry = DateTime.UtcNow.AddMinutes(15);
            _userService.SetResetToken(user.Email, resetToken, expiry);

            var delivery = await _emailService.SendAsync(
                user.Email,
                "Reset your SkillNet password",
                $"Your SkillNet password reset token is:\n\n{resetToken}\n\nThis token expires in 15 minutes. If you did not request a password reset, you can ignore this email.",
                "Password Reset");

            if (!delivery.Succeeded)
            {
                _logger.LogWarning(
                    "Password reset email was not delivered for user {UserId}: {Reason}",
                    user.UserID,
                    delivery.ErrorMessage);
            }

            return Ok(new { Message = responseMessage });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
                return BadRequest(new { Message = "Invalid email or token." });

            if (user.ResetToken != request.Token || !user.ResetTokenExpiry.HasValue || user.ResetTokenExpiry.Value < DateTime.UtcNow)
                return BadRequest(new { Message = "Invalid or expired token." });

            var policyError = _authService.ValidatePasswordPolicy(request.NewPassword);
            if (policyError != null)
                return BadRequest(new { Message = policyError });

            string newHash = _passwordHashService.HashPassword(request.NewPassword);
            _userService.ResetPassword(user.UserID, newHash);
            _userService.ResetFailedAttempts(user.Email);

            return Ok(new { Message = "Password has been reset successfully." });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var email = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { Message = "No user identity found in claims." });

            var user = _userService.GetUserByEmail(email);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            var roles = _userService.GetUserRoles(user.UserID);

            return Ok(new
            {
                user.UserID,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Phone,
                user.Status,
                Roles = roles
            });
        }
    }
}
