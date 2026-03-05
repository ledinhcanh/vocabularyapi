namespace API.Models.Request.Topic
{
    public class CreateTopicRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateTopicRequest : CreateTopicRequest
    {
        public int Id { get; set; }
    }
}