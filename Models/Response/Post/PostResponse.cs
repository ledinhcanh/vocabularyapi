namespace API.Models.Response.Post
{
    public class PostResponse
    {
        public int PostId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? ViewCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Slug { get; set; }
        public string? CategorySeo { set; get; }
        public string? ShareUrl { set; get; }
    }
}
