using System.ComponentModel.DataAnnotations;

namespace API.Models.Request.BaseRequest
{
    public class PagingRequest
    {        
        [Range(0, int.MaxValue, ErrorMessage = "PageIndex must be greater than or equal to zero")]
        public int PageIndex { set; get; }
        [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50")]
        public int PageSize { set; get; }
    }
}
