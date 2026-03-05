using System.ComponentModel.DataAnnotations;
using API.Models.Request.BaseRequest;

namespace API.Models.Request.Post
{
    public class GetPostRequest : PagingRequest
    {
        //public bool IsHot { set; get; }
        //public bool IsNew { set; get; }
        public int? PostId { get; set; }
        [MinLength(3)]
        public string? KeySearch { set; get; }
    }
}
