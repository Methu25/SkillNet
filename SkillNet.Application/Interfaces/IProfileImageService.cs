namespace SkillNet.Application.Interfaces
{
    public interface IProfileImageService
    {
        Task<string> UploadAsync(
            int userId,
            Stream content,
            string fileName,
            string contentType,
            long fileSize);

        Task<bool> DeleteAsync(int userId);
    }
}
