using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig;
using API.Models.Request.PublicBudget;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/web-link")]
    [ApiController]
    [Authorize]
    public class WebLinkController : AppControllerBase
    {
        private readonly IWebLinkRepository _repository;
        private readonly ILogger<WebLinkController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        public WebLinkController(IWebLinkRepository repository, ILogger<WebLinkController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._repository = repository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpGet("get-list-web-link")]
        public async Task<IActionResult> GetListWebLink()
        {
            var CacheKey = "api/web-link/get-list-web-link";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _repository.GetListWebLink();
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
