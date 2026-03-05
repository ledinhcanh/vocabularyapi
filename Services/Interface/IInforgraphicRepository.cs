using API.Models.AppConfig;

namespace API.Services.Interface
{
    public interface IInforgraphicRepository
    {
        public Task<AppResponse<object>> GetInforgraphics();
    }
}
