namespace SkillNet.Domain.Entities
{
    public class RefreshToken
    {
        public int TokenID { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
