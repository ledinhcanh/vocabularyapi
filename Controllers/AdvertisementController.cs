using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/advertisement")]
    [ApiController]
    [Authorize]
    public class AdvertisementController : AppControllerBase
    {
        private readonly IAdvertisementRepository _advertisementRepository;
        private readonly ILogger<AdvertisementController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public AdvertisementController(IAdvertisementRepository advertisementRepository, ILogger<AdvertisementController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._advertisementRepository = advertisementRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }
        [HttpGet("get-content/{PageId}")]
        public async Task<IActionResult> GetArticleDetail(int PageId)
        {
            var CacheKey = "api/advertisement/get-content: " + PageId;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _advertisementRepository.GetAdvertisementByPage(PageId);
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
