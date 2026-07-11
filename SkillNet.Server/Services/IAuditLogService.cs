namespace SkillNet.Server.Services
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string action, string? entity = null, int? entityId = null, string? oldValue = null, string? newValue = null);
    }
}
