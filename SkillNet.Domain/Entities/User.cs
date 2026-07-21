namespace SkillNet.Domain.Entities
{
    public class User
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int UserID { get; set; }
        public int UserId
        {
            get => UserID;
            set => UserID = value;
        }

        public string? Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";
        public int RoleId { get; set; }
        public int? OrganizationId { get; set; }
        public int? DepartmentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Candidate? Candidate { get; set; }
        public ICollection<ApplicationStatusHistory> ApplicationStatusChanges { get; set; } = new List<ApplicationStatusHistory>();
    }
}
