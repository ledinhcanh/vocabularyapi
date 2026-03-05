using API.Models.Request.BaseRequest;

namespace API.Models.Request.Post
{
    public class GetArticleFeedbackRequest : PagingRequest
    {
        public int ArticleId { set; get; }
    }
}
