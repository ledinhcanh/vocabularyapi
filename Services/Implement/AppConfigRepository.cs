using Newtonsoft.Json;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class AppConfigRepository : IAppConfigRepository
    {
        private readonly GovHniContext _context;
        private readonly IArticleRepository _articleRepository;
        private readonly int _siteId;
        private readonly string? _webDomain;
        private readonly string? _administrativeProceduresKey;
        public AppConfigRepository(GovHniContext GovHniContext, IConfiguration configuration, IArticleRepository articleRepository, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._administrativeProceduresKey = configuration.GetValue<string>("AppSettings:AdministrativeProceduresKey");
            this._articleRepository = articleRepository;
        }

        public async Task<AppResponse<object>> GetAdministrativeProcedures()
        {
            var data = await _context.AppMobileSettings.FindAsync(this._administrativeProceduresKey, this._siteId);
            if (data == null || string.IsNullOrEmpty(data.KeySettingValue))
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy dữ liệu"
                };
            return new AppResponse<object>
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(data.KeySettingValue)
            };
        }

        public async Task<AppResponse<AppMobileConfig>> GetConfigByKey(string key)
        {
            var Config = await _context.AppMobileConfigs.FindAsync(key, this._siteId);
            if (key == null)
            {
                return new AppResponse<AppMobileConfig>()
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy dữ liệu"
                };
            }
            return new AppResponse<AppMobileConfig>()
            {
                Data = Config,
                Message = "Thành công",
                IsSuccess = true
            };
        }

        public async Task<AppConfigResponse<object>> GetDataConfigByKey(string key)
        {
            var ConfigResult = await GetConfigByKey(key);
            if (!ConfigResult.IsSuccess || ConfigResult.Data == null)
            {
                return new AppConfigResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Key not found"
                };
            }
            switch (ConfigResult.Data.KeyName)
            {
                case AppConfigState.TRANGCHU_ARTICLE:
                    return await _articleRepository.GetArticlesByConfig(ConfigResult.Data);
            }
            return new AppConfigResponse<object>()
            {
                IsSuccess = false,
                Message = "Key chưa được xử lý"
            };
        }

        public async Task<AppResponse<object>> GetSettingByKey(string Key)
        {
            var data = await _context.AppMobileSettings.FindAsync(Key, this._siteId);
            if (data == null) return new AppResponse<object>()
            {
                IsSuccess = false,
                Message = "Key not found"
            };
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = data
            }; 
        }
    }
}
