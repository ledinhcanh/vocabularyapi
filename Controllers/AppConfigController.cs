using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig;
using API.Models.Database.Identities;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/app-config")]
    [ApiController]
    [Authorize]
    public class AppConfigController : AppControllerBase
    {
        private readonly IAppConfigRepository _appConfigRepository;
        private readonly ILogger<AppConfigController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public AppConfigController(IAppConfigRepository appConfigRepository, ILogger<AppConfigController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._appConfigRepository = appConfigRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }
        [HttpGet("get-administrative-procedures")]
        public async Task<IActionResult> GetConfigByKey()
        {
            var CacheKey = "api/app-config/get-administrative-procedures";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _appConfigRepository.GetAdministrativeProcedures();
            if (_enableCache)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
                };
                _memoryCache.Set(CacheKey, Data, cacheEntryOptions);
            }
            return Ok(Data);
        }
        [HttpGet("get-config/{key}")]
        public async Task<IActionResult> GetConfigByKey(string key)
        {
            var CacheKey = "api/app-config/get-config: " + key;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<AppMobileConfig> result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _appConfigRepository.GetConfigByKey(key);
            if (_enableCache)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
                };
                _memoryCache.Set(CacheKey, Data, cacheEntryOptions);
            }
            return Ok(Data);
        }
        [HttpGet("get-data/{key}")]
        public async Task<IActionResult> GetDataConfigByKey(string key)
        {
            var CacheKey = "api/app-config/get-data: " + key;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppConfigResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _appConfigRepository.GetDataConfigByKey(key);
            if (_enableCache)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
                };
                _memoryCache.Set(CacheKey, Data, cacheEntryOptions);
            }
            return Ok(Data);
        }
        [HttpGet("get-setting/{key}")]
        public async Task<IActionResult> GetSettingByKey(string key)
        {
            var CacheKey = "api/app-config/get-setting: " + key;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _appConfigRepository.GetSettingByKey(key);
            if (_enableCache)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
                };
                _memoryCache.Set(CacheKey, Data, cacheEntryOptions);
            }
            return Ok(Data);
        }
    }
}
