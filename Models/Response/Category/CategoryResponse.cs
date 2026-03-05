namespace API.Models.Response.Category
{
    public class TopicResponse
    {
        public int CategoryId { get; set; }
        public int? ParentId { set; get; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool? IsVisible { get; set; }
    }
}
