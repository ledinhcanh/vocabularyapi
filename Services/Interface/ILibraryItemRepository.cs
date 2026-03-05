using API.Models.AppConfig;
using API.Models.Request.BaseRequest;
using API.Models.Request.LibraryItem;

namespace API.Services.Interface
{
    public interface ILibraryItemRepository
    {
        public Task<AppPagingResponse<object>> GetLibraryItems(RequestGetLibraryItem request);
        public Task<AppResponse<object>> GetLibraryItemDetail(int itemId);
        public Task<AppResponse<object>> GetDefaultItemDetail();
        public Task<AppResponse<object>> GetLibraryItemRelated(int itemId);
    }
}
