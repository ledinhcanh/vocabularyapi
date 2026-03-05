using API.Models.AppConfig;

namespace API.Services.Interface
{
    public interface IWebLinkRepository
    {
        Task<AppResponse<object>> GetListWebLink();
    }
}
