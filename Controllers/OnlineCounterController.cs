using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/online-counter")]
    [ApiController]
    [Authorize]
    public class OnlineCounterController : AppControllerBase
    {
        private readonly IOnlineCounterRepository _repository;
        private readonly ILogger<OnlineCounterController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        public OnlineCounterController(IOnlineCounterRepository repository, ILogger<OnlineCounterController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._repository = repository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpGet("get-online-counter")]
        public async Task<IActionResult> GetOnlineCounter()
        {
            var CacheKey = "api/online-counter/get-online-counter";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _repository.GetOnlineCounter();
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
        [HttpPost("push-online-counter")]
        public async Task<IActionResult> PushOnlineCounter()
        {
            var CacheKey = "api/online-counter/push-online-counter: " + HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _repository.PushOnlineCounter();
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
