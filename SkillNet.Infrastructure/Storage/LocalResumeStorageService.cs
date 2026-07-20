using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Storage
{
    public class LocalResumeStorageService : IResumeStorageService
    {
        private const string ReferencePrefix = "/uploads/resumes/";
        private readonly string _storageDirectory;
        private readonly ILogger<LocalResumeStorageService> _logger;

        public LocalResumeStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<LocalResumeStorageService> logger)
        {
            var configuredDirectory = configuration["ResumeStorage:UploadDirectory"] ??
                "Storage/Resumes";
            _storageDirectory = Path.GetFullPath(Path.Combine(
                environment.ContentRootPath,
                configuredDirectory));
            _logger = logger;
        }

        public async Task<string> SaveAsync(Stream content)
        {
            Directory.CreateDirectory(_storageDirectory);
            var fileName = $"{Guid.NewGuid():N}.pdf";
            var filePath = GetValidatedPhysicalPath(fileName);

            try
            {
                await using var output = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    FileOptions.Asynchronous);
                await content.CopyToAsync(output);
                return $"{ReferencePrefix}{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write a resume file to local storage.");
                throw;
            }
        }

        public async Task<string> ReplaceAsync(string currentFileReference, Stream content)
        {
            var newFileReference = await SaveAsync(content);
            await DeleteAsync(currentFileReference);
            return newFileReference;
        }

        public Task DeleteAsync(string fileReference)
        {
            var filePath = GetPhysicalPathFromReference(fileReference);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Resume file was already missing from local storage: {Reference}", fileReference);
                return Task.CompletedTask;
            }

            try
            {
                File.Delete(filePath);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete resume file {Reference}.", fileReference);
                throw;
            }
        }

        public Task<Stream?> OpenReadAsync(string fileReference)
        {
            var filePath = GetPhysicalPathFromReference(fileReference);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Resume file was not found in local storage: {Reference}", fileReference);
                return Task.FromResult<Stream?>(null);
            }

            try
            {
                Stream stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                return Task.FromResult<Stream?>(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open resume file {Reference}.", fileReference);
                throw;
            }
        }

        private string GetPhysicalPathFromReference(string fileReference)
        {
            if (!fileReference.StartsWith(ReferencePrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid resume storage reference.");
            }

            var fileName = Path.GetFileName(fileReference);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("Invalid resume storage reference.");
            }

            return GetValidatedPhysicalPath(fileName);
        }

        private string GetValidatedPhysicalPath(string fileName)
        {
            var filePath = Path.GetFullPath(Path.Combine(_storageDirectory, fileName));
            var expectedPrefix = _storageDirectory.TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            if (!filePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid resume storage path.");
            }

            return filePath;
        }
    }
}
