using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateRefreshTokenAsync(int userId, string token, DateTime expiresAt);
        void CreateRefreshToken(int userId, string token, DateTime expiresAt);

        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        RefreshToken? GetRefreshToken(string token);

        Task RevokeRefreshTokenAsync(string token);
        void RevokeRefreshToken(string token);
    }
}
