using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig; 
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/banner-footer")]
    [ApiController]
    [Authorize]
    public class BannerFooterController : AppControllerBase
    {
        private readonly IBannerFooterRepository _bannerFooterRepository;
        private readonly ILogger<BannerFooterController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public BannerFooterController(IBannerFooterRepository bannerFooterRepository, ILogger<BannerFooterController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._bannerFooterRepository = bannerFooterRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }
        [HttpGet("get-banner")]
        public async Task<IActionResult> GetBanner()
        {
            var CacheKey = "get-banner";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _bannerFooterRepository.GetBanner();
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
        [HttpGet("get-default-image-banner")]
        public async Task<IActionResult> GetDefaultImagesBanner()
        {
            var CacheKey = "get-default-image-banner";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _bannerFooterRepository.GetDefaultImagesBanner();
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
        [HttpGet("get-footer")]
        public async Task<IActionResult> GetFooter()
        {
            var CacheKey = "get-footer";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _bannerFooterRepository.GetFooter();
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
