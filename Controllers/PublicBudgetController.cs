using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.Request.Document;
using API.Models.Request.PublicBudget;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/public-budget")]
    [ApiController]
    [Authorize]
    public class PublicBudgetController : AppControllerBase
    {
        private readonly IPublicBudgetRepository _publicBudgetRepository;
        private readonly ILogger<PublicBudgetController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        public PublicBudgetController(IPublicBudgetRepository publicBudgetRepository, ILogger<PublicBudgetController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._publicBudgetRepository = publicBudgetRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }
        [HttpGet("get-categories")]
        public async Task<IActionResult> GetCategories()
        {
            var CacheKey = "api/public-budget/GetCategories";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _publicBudgetRepository.GetCategories();
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
        [HttpGet("get-kybaocao")]
        public async Task<IActionResult> GetKyBaoCaos()
        {
            var CacheKey = "api/public-budget/get-kybaocao";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _publicBudgetRepository.GetKyBaoCaos();
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
        [HttpPost("get-list-public-budget")]
        public async Task<IActionResult> GetListPublicBudget([FromBody] GetPublicBudgetRequest request)
        {
            var CacheKey = "api/public-budget/get-list-public-budget:" + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppPagingResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _publicBudgetRepository.GetListPublicBudget(request);
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

        [HttpGet("get-public-budget-detail/{id}")]
        public async Task<IActionResult> GetPublicBudgetDetail(int id)
        {
            var CacheKey = "api/document/get-list-documents:" + id;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _publicBudgetRepository.GetPublicBudgetDetail(id);
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
