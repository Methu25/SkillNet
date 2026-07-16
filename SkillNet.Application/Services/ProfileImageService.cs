using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class ProfileImageService : IProfileImageService
    {
        private const long MaximumFileSize = 5 * 1024 * 1024;

        private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["image/jpeg"] = ".jpg",
                ["image/png"] = ".png",
                ["image/webp"] = ".webp"
            };

        private readonly ICandidateRepository _candidateRepository;
        private readonly IProfileImageStorageService _storageService;
        private readonly ICandidateService _candidateService;
        private readonly ICandidateNotificationService _notificationService;

        public ProfileImageService(
            ICandidateRepository candidateRepository,
            IProfileImageStorageService storageService,
            ICandidateService candidateService,
            ICandidateNotificationService notificationService)
        {
            _candidateRepository = candidateRepository;
            _storageService = storageService;
            _candidateService = candidateService;
            _notificationService = notificationService;
        }

        public async Task<string> UploadAsync(
            int userId,
            Stream content,
            string fileName,
            string contentType,
            long fileSize)
        {
            var candidate = await _candidateRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate profile {userId} was not found.");
            }
            var previousCompletion = await GetCompletionAsync(userId);

            var extension = ValidateImage(content, fileName, contentType, fileSize);
            var previousImageUrl = candidate.ProfileImagePath;
            var newImageUrl = await _storageService.SaveAsync(content, extension);

            try
            {
                candidate.ProfileImagePath = newImageUrl;
                candidate.UpdatedDate = DateTime.UtcNow;
                await _candidateRepository.UpdateCandidateAsync(candidate);
            }
            catch
            {
                await _storageService.DeleteAsync(newImageUrl);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(previousImageUrl))
            {
                await _storageService.DeleteAsync(previousImageUrl);
            }

            await NotifyCompletionChangeAsync(userId, previousCompletion);

            return newImageUrl;
        }

        private async Task<int> GetCompletionAsync(int userId)
        {
            return (await _candidateService.GetCandidateProfileAsync(userId))?
                .ProfileCompletion.CompletionPercentage ?? 0;
        }

        private async Task NotifyCompletionChangeAsync(int userId, int previousPercentage)
        {
            var currentPercentage = await GetCompletionAsync(userId);
            await _notificationService.NotifyProfileProgressAsync(
                userId, previousPercentage, currentPercentage);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var candidate = await _candidateRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate profile {userId} was not found.");
            }

            if (string.IsNullOrWhiteSpace(candidate.ProfileImagePath))
            {
                return false;
            }

            var imageUrl = candidate.ProfileImagePath;
            candidate.ProfileImagePath = null;
            candidate.UpdatedDate = DateTime.UtcNow;
            await _candidateRepository.UpdateCandidateAsync(candidate);
            await _storageService.DeleteAsync(imageUrl);
            return true;
        }

        private static string ValidateImage(
            Stream content,
            string fileName,
            string contentType,
            long fileSize)
        {
            if (content == null || content == Stream.Null || !content.CanRead || fileSize <= 0)
            {
                throw new ArgumentException("A non-empty profile image is required.");
            }

            if (fileSize > MaximumFileSize)
            {
                throw new ArgumentException($"Profile image size cannot exceed {MaximumFileSize} bytes.");
            }

            if (!AllowedContentTypes.TryGetValue(contentType, out var normalizedExtension))
            {
                throw new ArgumentException("Only JPEG, PNG, and WEBP profile images are supported.");
            }

            var suppliedExtension = Path.GetExtension(fileName);
            var extensionMatches = normalizedExtension == ".jpg"
                ? suppliedExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                  suppliedExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                : suppliedExtension.Equals(normalizedExtension, StringComparison.OrdinalIgnoreCase);

            if (!extensionMatches || !HasValidSignature(content, normalizedExtension))
            {
                throw new ArgumentException("The profile image type or content is invalid.");
            }

            return normalizedExtension;
        }

        private static bool HasValidSignature(Stream content, string extension)
        {
            var originalPosition = content.CanSeek ? content.Position : 0;
            Span<byte> header = stackalloc byte[12];
            var bytesRead = content.Read(header);
            if (content.CanSeek)
            {
                content.Position = originalPosition;
            }

            return extension switch
            {
                ".jpg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => bytesRead >= 8 && header[..8].SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                ".webp" => bytesRead >= 12 &&
                    header[..4].SequenceEqual("RIFF"u8) &&
                    header[8..12].SequenceEqual("WEBP"u8),
                _ => false
            };
        }
    }
}
