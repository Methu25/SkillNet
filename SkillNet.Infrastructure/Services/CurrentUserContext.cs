using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.Infrastructure.Services
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public int? UserId
        {
            get
            {
                var email = Email;
                if (string.IsNullOrEmpty(email)) return null;
                var user = _userService.GetUserByEmail(email);
                return user?.UserID;
            }
        }

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }
    }
}
