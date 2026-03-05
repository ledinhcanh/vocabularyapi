using API.Models.Request.BaseRequest;

namespace API.Models.Request.LibraryItem
{
    public class RequestGetLibraryItem : PagingRequest
    {
        public List<string>? ItemType { set; get; }
    }
}
