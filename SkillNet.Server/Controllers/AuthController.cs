using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Server.Models;
using SkillNet.Server.Services;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthenticationService authService,
        IUserService userService,
        IJwtTokenService jwtTokenService,
        IPasswordHashService passwordHashService) : ControllerBase
    {
        private readonly IAuthenticationService _authService = authService;
        private readonly IUserService _userService = userService;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
        private readonly IPasswordHashService _passwordHashService = passwordHashService;

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!_authService.Register(request, out string error))
            {
                return BadRequest(new { Message = error });
            }

            return Ok(new { Message = "Registration successful!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var response = _authService.Login(request, out string error);
            if (response == null)
            {
                return Unauthorized(new { Message = error });
            }

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { Message = "Refresh token required." });
            }

            _jwtTokenService.RevokeRefreshToken(request.RefreshToken);
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { Message = "Refresh token required." });
            }

            var userId = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
            if (!userId.HasValue)
            {
                return Unauthorized(new { Message = "Invalid or expired refresh token." });
            }

            var user = _userService.GetUserById(userId.Value);
            if (user == null || user.Status != "Active")
            {
                return Unauthorized(new { Message = "User inactive or not found." });
            }

            // Revoke the old refresh token
            _jwtTokenService.RevokeRefreshToken(request.RefreshToken);

            // Generate new pair
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
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                // Return generic success to prevent email enumeration
                return Ok(new { Message = "If the email is registered, a password reset token has been sent." });
            }

            // Generate a secure reset token
            string resetToken = Guid.NewGuid().ToString("N");
            DateTime expiry = DateTime.UtcNow.AddMinutes(15); // 15-minute expiry
            _userService.SetResetToken(user.Email, resetToken, expiry);

            // Log/simulate sending email (print to debug output)
            Console.WriteLine($"[EMAIL SIMULATION] To: {user.Email} | Subject: Password Reset Token | Token: {resetToken}");

            // For phase 1 development & UI ease-of-use, we return the token in the response body 
            // so frontend developers can test easily. In production, this would only be emailed.
            return Ok(new
            {
                Message = "If the email is registered, a password reset token has been sent.",
                DebugToken = resetToken
            });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid email or token." });
            }

            if (user.ResetToken != request.Token || !user.ResetTokenExpiry.HasValue || user.ResetTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest(new { Message = "Invalid or expired token." });
            }

            // Validate new password policy
            var policyError = _authService.ValidatePasswordPolicy(request.NewPassword);
            if (policyError != null)
            {
                return BadRequest(new { Message = policyError });
            }

            // Reset password
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
            {
                return Unauthorized(new { Message = "No user identity found in claims." });
            }

            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

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