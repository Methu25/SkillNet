namespace SkillNet.Application.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
    }
}
