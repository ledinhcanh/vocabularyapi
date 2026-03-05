using API.Models.AppConfig;
using API.Models.Database.Identities;

namespace API.Services.Interface
{
    public interface IAppConfigRepository
    {
        Task<AppResponse<AppMobileConfig>> GetConfigByKey(string key);
        Task<AppConfigResponse<object>> GetDataConfigByKey(string key);
        Task<AppResponse<object>> GetAdministrativeProcedures();
        Task<AppResponse<object>> GetSettingByKey(string Key);
    }
}
