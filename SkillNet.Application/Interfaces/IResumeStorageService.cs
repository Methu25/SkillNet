namespace SkillNet.Application.Interfaces
{
    public interface IResumeStorageService
    {
        Task<string> SaveAsync(Stream content);
        Task<string> ReplaceAsync(string currentFileReference, Stream content);
        Task DeleteAsync(string fileReference);
        Task<Stream?> OpenReadAsync(string fileReference);
    }
}
