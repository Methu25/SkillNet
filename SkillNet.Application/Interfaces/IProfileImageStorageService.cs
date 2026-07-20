namespace SkillNet.Application.Interfaces
{
    public interface IProfileImageStorageService
    {
        Task<string> SaveAsync(Stream content, string fileExtension);
        Task<string> ReplaceAsync(string currentImageUrl, Stream content, string fileExtension);
        Task DeleteAsync(string imageUrl);
    }
}
