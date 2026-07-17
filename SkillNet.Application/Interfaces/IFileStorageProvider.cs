using System.IO;

namespace SkillNet.Application.Interfaces
{
    public interface IFileStorageProvider
    {
        Task<string> SaveAsync(Stream content, string fileName, string contentType);
        Task<string> ReplaceAsync(
            string existingFileReference,
            Stream content,
            string fileName,
            string contentType);
        Task DeleteAsync(string filePath);
        Task<string> GetDownloadReferenceAsync(string fileReference);
    }

    public interface IFileStorageProviderFactory
    {
        IFileStorageProvider Create(string providerName);
    }
}
