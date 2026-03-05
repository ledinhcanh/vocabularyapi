using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Services.Interface;
using API.Models.Request.Category;
using Microsoft.AspNetCore.Authorization;
using API.Models.AppConfig;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize] 
    public class CategoryController : AppControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        private const string CACHE_KEY_GET_CATEGORIES = "api/categories";

        public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._categoryRepository = categoryRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            _logger.Log(LogLevel.Information, CACHE_KEY_GET_CATEGORIES);

            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CACHE_KEY_GET_CATEGORIES, out AppResponse<object>? result))
            {
                Response.Headers.Append("Cache", "true");
                return Ok(result);
            }

            var data = await _categoryRepository.GetCategories();

            if (_enableCache)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
                    Priority = CacheItemPriority.Normal
                };
                _memoryCache.Set(CACHE_KEY_GET_CATEGORIES, data, cacheEntryOptions);
            }
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            //var CacheKey = $"api/categories/{id}";

            //_logger.Log(LogLevel.Information, CacheKey);

            //if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            //{
            //    Response.Headers.Append("Cache", "true");
            //    return Ok(result);
            //}

            var data = await _categoryRepository.GetCategoryById(id);
            if (!data.IsSuccess) return NotFound(data);
            //if (_enableCache)
            //{
            //    var cacheEntryOptions = new MemoryCacheEntryOptions
            //    {
            //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
            //        Priority = CacheItemPriority.Normal
            //    };
            //    _memoryCache.Set(CacheKey, data, cacheEntryOptions);
            //}

            return Ok(data);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _categoryRepository.CreateCategory(request);

            if (result.IsSuccess)
            {
                _memoryCache.Remove(CACHE_KEY_GET_CATEGORIES);
            }

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _categoryRepository.UpdateCategory(request);

            if (result.IsSuccess)
            {
                _memoryCache.Remove(CACHE_KEY_GET_CATEGORIES);
            }

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryRepository.DeleteCategory(id);

            if (result.IsSuccess)
            {
                _memoryCache.Remove(CACHE_KEY_GET_CATEGORIES);
            }

            return Ok(result);
        }
    }
}