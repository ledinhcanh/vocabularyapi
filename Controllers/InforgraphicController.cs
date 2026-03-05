using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/inforgraphic")]
    [ApiController]
    [Authorize]
    public class InforgraphicController : AppControllerBase
    {
        private readonly IInforgraphicRepository _reposioty;
        private readonly ILogger<InforgraphicController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public InforgraphicController(IInforgraphicRepository reposioty, ILogger<InforgraphicController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._reposioty = reposioty;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpGet("get-inforgraphics")]
        public async Task<IActionResult> GetCommonCatalog()
        {
            var CacheKey = "api/inforgraphic/get-inforgraphics";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _reposioty.GetInforgraphics();
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
