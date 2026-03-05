using API.Models.AppConfig;

namespace API.Services.Interface
{
    public interface IAdvertisementRepository
    {
        Task<AppResponse<object>> GetAdvertisementByPage(int pageId);
    }
}
