using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
//using API.Models.Database.Identities;

namespace API.Models.AppConfig
{
    public class AppResponse<T>
    {
        public bool IsSuccess { set; get; }
        public string? Message { set; get; }
        public T? Data { set; get; }
    } 
    public class AppPagingResponse<T>
    {
        public bool IsSuccess { set; get; }
        public string? Message { set; get; }
        public PagingResponse? Paging { set; get; }
        public T? Data { set; get; }
    }
    public class PagingResponse
    {
        public int PageIndex { set; get; }
        [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50")]
        public int PageSize { set; get; }
        public int CurrentItemCount { set; get; }
        public int TotalRows { set; get; }
        public int TotalPages => (int) Math.Ceiling(decimal.Divide(TotalRows, PageSize));
    }
}
