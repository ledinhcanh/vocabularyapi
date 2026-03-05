using API.Models.AppConfig;

namespace API.Services.Interface
{
    public interface IPortalRepository
    {
        Task<AppResponse<object>> GetPortalList();
    }
}
