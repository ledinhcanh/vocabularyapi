using API.Models.Request.BaseRequest;

namespace API.Models.Request.Post
{
    public class GetPostByCatRequest : PagingRequest
    {
        public int ArticleCatId { set; get; }
    }
}
