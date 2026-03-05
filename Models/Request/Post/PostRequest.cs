using System.ComponentModel.DataAnnotations;

namespace API.Models.Request.Post
{
    public class CreatePostRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(250, ErrorMessage = "Tiêu đề không quá 250 ký tự")]
        public string Title { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int CategoryId { get; set; }
        public bool IsPublished { get; set; }

    }

    public class UpdatePostRequest : CreatePostRequest
    {
        public int PostId { get; set; }
    }
}
