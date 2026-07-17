using System.ComponentModel.DataAnnotations;

namespace SkillNet.WebApi.Models
{
    public class FileUploadRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
