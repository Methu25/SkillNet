using Microsoft.AspNetCore.Hosting;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Storage
{
    public class LocalProfileImageStorageService : IProfileImageStorageService
    {
        private const string UploadUrlPrefix = "/uploads/profile-images/";
        private readonly string _storageDirectory;

        public LocalProfileImageStorageService(IWebHostEnvironment environment)
        {
            var webRootPath = environment.WebRootPath ??
                Path.Combine(environment.ContentRootPath, "wwwroot");
            _storageDirectory = Path.GetFullPath(
                Path.Combine(webRootPath, "uploads", "profile-images"));
        }

        public async Task<string> SaveAsync(Stream content, string fileExtension)
        {
            Directory.CreateDirectory(_storageDirectory);
            var fileName = $"{Guid.NewGuid():N}{fileExtension.ToLowerInvariant()}";
            var filePath = GetValidatedPhysicalPath(fileName);

            await using var output = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous);
            await content.CopyToAsync(output);

            return $"{UploadUrlPrefix}{fileName}";
        }

        public async Task<string> ReplaceAsync(
            string currentImageUrl,
            Stream content,
            string fileExtension)
        {
            var newImageUrl = await SaveAsync(content, fileExtension);
            await DeleteAsync(currentImageUrl);
            return newImageUrl;
        }

        public Task DeleteAsync(string imageUrl)
        {
            if (!TryGetOwnedFileName(imageUrl, out var fileName))
            {
                return Task.CompletedTask;
            }

            var filePath = GetValidatedPhysicalPath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        private string GetValidatedPhysicalPath(string fileName)
        {
            var filePath = Path.GetFullPath(Path.Combine(_storageDirectory, fileName));
            var expectedPrefix = _storageDirectory.TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            if (!filePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid profile image storage path.");
            }

            return filePath;
        }

        private static bool TryGetOwnedFileName(string imageUrl, out string fileName)
        {
            fileName = string.Empty;
            if (!imageUrl.StartsWith(UploadUrlPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fileName = Path.GetFileName(imageUrl);
            return !string.IsNullOrWhiteSpace(fileName);
        }
    }
}
