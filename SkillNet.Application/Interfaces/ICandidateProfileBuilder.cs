using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface ICandidateProfileBuilder
    {
        ICandidateProfileBuilder SetBasicInformation(
            string firstName,
            string lastName,
            string? phoneNumber,
            string? location);

        ICandidateProfileBuilder SetProfessionalInformation(
            string? professionalTitle,
            string? professionalSummary);

        ICandidateProfileBuilder SetEducation(
            string? education,
            string? degree,
            string? university);

        ICandidateProfileBuilder SetExperience(int? experienceYears);

        Candidate Build(int userId);
    }
}
