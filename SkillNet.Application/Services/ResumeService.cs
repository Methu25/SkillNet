using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace SkillNet.Application.Services
{
    public class ResumeService : IResumeService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly IResumeStorageService _storageService;
        private readonly long _maximumFileSize;
        private readonly string _allowedContentType;

        public ResumeService(
            ICandidateRepository candidateRepository,
            IResumeRepository resumeRepository,
            IResumeStorageService storageService,
            IConfiguration configuration)
        {
            _candidateRepository = candidateRepository;
            _resumeRepository = resumeRepository;
            _storageService = storageService;
            _maximumFileSize = long.TryParse(
                configuration["ResumeStorage:MaximumFileSizeBytes"],
                out var configuredMaximum)
                    ? configuredMaximum
                    : 10 * 1024 * 1024;
            _allowedContentType = configuration["ResumeStorage:AllowedMimeType"] ??
                "application/pdf";
        }

        public async Task<IEnumerable<ResumeDto>> GetCandidateResumesAsync(int candidateId)
        {
            await EnsureCandidateExistsAsync(candidateId);
            var resumes = await _resumeRepository.GetAllResumesByCandidateIdAsync(candidateId);
            return await Task.WhenAll(resumes.Select(MapToResumeDtoAsync));
        }

        public async Task<ResumeDto?> GetActiveResumeAsync(int candidateId)
        {
            await EnsureCandidateExistsAsync(candidateId);
            var resume = await _resumeRepository.GetActiveResumeByCandidateIdAsync(candidateId);
            return resume == null ? null : await MapToResumeDtoAsync(resume);
        }

        public async Task<ResumeDto> UploadResumeAsync(int candidateId, UploadResumeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            await EnsureCandidateExistsAsync(candidateId);
            ValidateFile(dto.FileName, dto.ContentType, dto.FileSize, dto.Content);

            var existingResumes = (await _resumeRepository
                .GetAllResumesByCandidateIdAsync(candidateId)).ToList();
            var safeFileName = Path.GetFileName(dto.FileName);
            var fileReference = await _storageService.SaveAsync(dto.Content);

            var resume = new Resume
            {
                CandidateId = candidateId,
                FileName = safeFileName,
                FilePath = fileReference,
                FileType = dto.ContentType,
                FileSize = dto.FileSize,
                UploadedDate = DateTime.UtcNow,
                IsActive = existingResumes.Count == 0
            };

            try
            {
                var createdResume = await _resumeRepository.AddResumeAsync(resume);
                return await MapToResumeDtoAsync(createdResume);
            }
            catch
            {
                await _storageService.DeleteAsync(fileReference);
                throw;
            }
        }

        public async Task<ResumeDto?> ReplaceResumeAsync(
            int candidateId,
            int resumeId,
            ReplaceResumeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            await EnsureCandidateExistsAsync(candidateId);
            ValidateFile(dto.FileName, dto.ContentType, dto.FileSize, dto.Content);

            var resume = await GetOwnedResumeAsync(candidateId, resumeId);
            if (resume == null)
            {
                return null;
            }

            var safeFileName = Path.GetFileName(dto.FileName);
            var previousFileReference = resume.FilePath;
            var newFileReference = await _storageService.SaveAsync(dto.Content);

            try
            {
                resume.FileName = safeFileName;
                resume.FilePath = newFileReference;
                resume.FileType = dto.ContentType;
                resume.FileSize = dto.FileSize;
                resume.UploadedDate = DateTime.UtcNow;

                await _resumeRepository.UpdateResumeAsync(resume);
            }
            catch
            {
                await _storageService.DeleteAsync(newFileReference);
                throw;
            }

            await _storageService.DeleteAsync(previousFileReference);
            return await MapToResumeDtoAsync(resume);
        }

        public async Task<ResumeDto?> SetActiveResumeAsync(int candidateId, int resumeId)
        {
            await EnsureCandidateExistsAsync(candidateId);

            var selectedResume = await GetOwnedResumeAsync(candidateId, resumeId);
            if (selectedResume == null)
            {
                return null;
            }

            var resumes = await _resumeRepository.GetAllResumesByCandidateIdAsync(candidateId);
            foreach (var resume in resumes.Where(resume => resume.IsActive && resume.ResumeId != resumeId))
            {
                resume.IsActive = false;
                await _resumeRepository.UpdateResumeAsync(resume);
            }

            if (!selectedResume.IsActive)
            {
                selectedResume.IsActive = true;
                await _resumeRepository.UpdateResumeAsync(selectedResume);
            }

            return await MapToResumeDtoAsync(selectedResume);
        }

        public async Task<bool> DeleteResumeAsync(int candidateId, int resumeId)
        {
            await EnsureCandidateExistsAsync(candidateId);

            var resume = await GetOwnedResumeAsync(candidateId, resumeId);
            if (resume == null)
            {
                return false;
            }

            var wasActive = resume.IsActive;
            var fileReference = resume.FilePath;

            await _resumeRepository.DeleteResumeAsync(resumeId);

            if (wasActive)
            {
                var remainingResumes = await _resumeRepository
                    .GetAllResumesByCandidateIdAsync(candidateId);
                var replacement = remainingResumes.FirstOrDefault();
                if (replacement != null)
                {
                    replacement.IsActive = true;
                    await _resumeRepository.UpdateResumeAsync(replacement);
                }
            }

            await _storageService.DeleteAsync(fileReference);

            return true;
        }

        public async Task<ResumeDownloadDto?> DownloadResumeAsync(int candidateId, int resumeId)
        {
            await EnsureCandidateExistsAsync(candidateId);

            var resume = await GetOwnedResumeAsync(candidateId, resumeId);
            if (resume == null)
            {
                return null;
            }

            var content = await _storageService.OpenReadAsync(resume.FilePath);
            if (content == null)
            {
                return null;
            }

            return new ResumeDownloadDto
            {
                Content = content,
                FileName = Path.GetFileName(resume.FileName),
                ContentType = _allowedContentType
            };
        }

        private async Task EnsureCandidateExistsAsync(int candidateId)
        {
            if (!await _candidateRepository.CandidateExistsAsync(candidateId))
            {
                throw new KeyNotFoundException($"Candidate profile {candidateId} was not found.");
            }
        }

        private async Task<Resume?> GetOwnedResumeAsync(int candidateId, int resumeId)
        {
            var resume = await _resumeRepository.GetResumeByIdAsync(resumeId);
            return resume?.CandidateId == candidateId ? resume : null;
        }

        private static Task<ResumeDto> MapToResumeDtoAsync(Resume resume)
        {
            return Task.FromResult(new ResumeDto
            {
                ResumeId = resume.ResumeId,
                CandidateId = resume.CandidateId,
                FileName = resume.FileName,
                FilePath = $"/api/candidate/resumes/{resume.ResumeId}/download",
                FileType = resume.FileType,
                FileSize = resume.FileSize,
                UploadedDate = resume.UploadedDate,
                IsActive = resume.IsActive
            });
        }

        private void ValidateFile(
            string fileName,
            string contentType,
            long fileSize,
            Stream content)
        {
            if (content == null || content == Stream.Null || !content.CanRead || fileSize <= 0)
            {
                throw new ArgumentException("A non-empty resume file is required.");
            }

            if (fileSize > _maximumFileSize)
            {
                throw new ArgumentException($"Resume file size cannot exceed {_maximumFileSize} bytes.");
            }

            if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(contentType, _allowedContentType, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only PDF resume files are supported.");
            }

            if (string.IsNullOrWhiteSpace(Path.GetFileName(fileName)))
            {
                throw new ArgumentException("A valid resume file name is required.", nameof(fileName));
            }

            var originalPosition = content.CanSeek ? content.Position : 0;
            Span<byte> signature = stackalloc byte[5];
            var bytesRead = content.Read(signature);
            if (content.CanSeek)
            {
                content.Position = originalPosition;
            }

            if (bytesRead < 5 || !signature.SequenceEqual("%PDF-"u8))
            {
                throw new ArgumentException("The uploaded file does not contain a valid PDF signature.");
            }
        }
    }
}
