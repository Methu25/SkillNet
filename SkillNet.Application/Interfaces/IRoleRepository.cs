namespace SkillNet.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<int?> GetRoleIdByNameAsync(string roleName);
        int? GetRoleIdByName(string roleName);

        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        bool AssignRoleToUser(int userId, int roleId);

        Task<List<string>> GetRolesByUserIdAsync(int userId);
        List<string> GetRolesByUserId(int userId);
    }
}
