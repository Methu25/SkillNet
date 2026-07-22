using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(string email, List<string> roles);
        string GenerateRefreshToken(int userId);
        int? ValidateRefreshToken(string refreshToken);
        void RevokeRefreshToken(string refreshToken);
    }


    public class JwtTokenService(
        IConfiguration config,
        IRefreshTokenRepository refreshTokenRepository,
        ISystemConfigurationService systemConfig) : IJwtTokenService
    {
        private readonly IConfiguration _config = config;
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
        private readonly ISystemConfigurationService _systemConfig = systemConfig;


        public string GenerateAccessToken(string email, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, email),
                new(JwtRegisteredClaimNames.Sub, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var sessionTimeout = _systemConfig.GetIntSetting("SessionTimeoutMinutes", 15);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(sessionTimeout),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(int userId)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            string tokenString = Convert.ToBase64String(randomNumber);

            DateTime expiresAt = DateTime.UtcNow.AddDays(7);
            _refreshTokenRepository.CreateRefreshToken(userId, tokenString, expiresAt);

            return tokenString;
        }

        public int? ValidateRefreshToken(string refreshToken)
        {
            var tokenEntity = _refreshTokenRepository.GetRefreshToken(refreshToken);
            if (tokenEntity != null)
            {
                if (tokenEntity.ExpiresAt > DateTime.UtcNow && !tokenEntity.IsRevoked)
                {
                    return tokenEntity.UserID;
                }
            }
            return null;
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _refreshTokenRepository.RevokeRefreshToken(refreshToken);
        }
    }
}
