using System.Collections.Generic;

namespace SkillNet.Server.Services
{
    public interface IAuthorizationService
    {
        bool UserHasRole(int userId, string roleName);
    }

    public class AuthorizationService(IUserService userService) : IAuthorizationService
    {
        private readonly IUserService _userService = userService;

        public bool UserHasRole(int userId, string roleName)
        {
            var roles = _userService.GetUserRoles(userId);
            return roles.Contains(roleName);
        }
    }
}
