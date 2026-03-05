namespace API.Models.Response.Topic
{
    public class TopicResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { set; get; }
        public string? ImageUrl { set; get; }
    }
}
