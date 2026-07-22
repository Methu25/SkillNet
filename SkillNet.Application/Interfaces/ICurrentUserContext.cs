namespace SkillNet.Application.Interfaces
{
    public interface ICurrentUserContext
    {
        int? UserId { get; }
        string? Email { get; }
        bool IsInRole(string role);
    }
}
