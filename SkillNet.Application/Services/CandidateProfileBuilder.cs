using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Services
{
    public class CandidateProfileBuilder : ICandidateProfileBuilder
    {
        private string? _firstName;
        private string? _lastName;
        private string? _phoneNumber;
        private string? _location;
        private string? _professionalTitle;
        private string? _professionalSummary;
        private string? _education;
        private string? _degree;
        private string? _university;
        private int? _experienceYears;

        public ICandidateProfileBuilder SetBasicInformation(
            string firstName,
            string lastName,
            string? phoneNumber,
            string? location)
        {
            _firstName = firstName;
            _lastName = lastName;
            _phoneNumber = phoneNumber;
            _location = location;
            return this;
        }

        public ICandidateProfileBuilder SetProfessionalInformation(
            string? professionalTitle,
            string? professionalSummary)
        {
            _professionalTitle = professionalTitle;
            _professionalSummary = professionalSummary;
            return this;
        }

        public ICandidateProfileBuilder SetEducation(
            string? education,
            string? degree,
            string? university)
        {
            _education = education;
            _degree = degree;
            _university = university;
            return this;
        }

        public ICandidateProfileBuilder SetExperience(int? experienceYears)
        {
            if (experienceYears < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(experienceYears),
                    "Experience years cannot be negative.");
            }

            _experienceYears = experienceYears;
            return this;
        }

        public Candidate Build(int userId)
        {
            ValidateMandatoryInformation();

            var now = DateTime.UtcNow;
            var candidate = new Candidate
            {
                UserId = userId,
                FirstName = _firstName!.Trim(),
                LastName = _lastName!.Trim(),
                PhoneNumber = _phoneNumber!.Trim(),
                Location = _location!.Trim(),
                ProfessionalTitle = NormalizeOptionalValue(_professionalTitle),
                ProfessionalSummary = NormalizeOptionalValue(_professionalSummary),
                Education = NormalizeOptionalValue(_education),
                Degree = NormalizeOptionalValue(_degree),
                University = NormalizeOptionalValue(_university),
                ExperienceYears = _experienceYears,
                CreatedDate = now,
                UpdatedDate = now
            };

            Reset();
            return candidate;
        }

        private void ValidateMandatoryInformation()
        {
            ValidateRequiredValue(_firstName, "First name");
            ValidateRequiredValue(_lastName, "Last name");
            ValidateRequiredValue(_phoneNumber, "Phone number");
            ValidateRequiredValue(_location, "Location");
        }

        private static void ValidateRequiredValue(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{fieldName} is required before building a candidate profile.");
            }
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private void Reset()
        {
            _firstName = null;
            _lastName = null;
            _phoneNumber = null;
            _location = null;
            _professionalTitle = null;
            _professionalSummary = null;
            _education = null;
            _degree = null;
            _university = null;
            _experienceYears = null;
        }
    }
}
