using API.Models.AppConfig;

namespace API.Services.Interface
{
    public interface IOnlineCounterRepository
    {
        public Task<AppResponse<object>> GetOnlineCounter();
        public Task<AppResponse<object>> PushOnlineCounter();
    }
}
