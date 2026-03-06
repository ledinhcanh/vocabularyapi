using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Services.Interface;
using API.Models.Request.Topic;
using Microsoft.AspNetCore.Authorization;
using API.Models.AppConfig;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Controllers
{
    [Route("api/topics")]
    [ApiController]
    [Authorize] 
    public class TopicController : AppControllerBase
    {
        private readonly ITopicRepository _topicRepository;
        private readonly ILogger<TopicController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        private const string CACHE_KEY_GET_CATEGORIES = "api/topics";

        public TopicController(ITopicRepository topicRepository, ILogger<TopicController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._topicRepository = topicRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopics([FromQuery] string? keyword = null)
        {
            var cacheKey = string.IsNullOrEmpty(keyword) ? CACHE_KEY_GET_CATEGORIES : $"{CACHE_KEY_GET_CATEGORIES}?keyword={keyword}";
            _logger.Log(LogLevel.Information, cacheKey);

            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(cacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Append("Cache", "true");
                return Ok(result);
            }

            var data = await _topicRepository.GetTopics(keyword);

            if (_enableCache)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this._timeExpireInMinutes),
                    Priority = CacheItemPriority.Normal
                };
                _memoryCache.Set(cacheKey, data, cacheEntryOptions);
            }
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var data = await _topicRepository.GetTopicById(id);
            if (!data.IsSuccess) return NotFound(data);

            return Ok(data);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTopicRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _topicRepository.CreateTopic(request);

            if (result.IsSuccess)
            {
                _memoryCache.Remove(CACHE_KEY_GET_CATEGORIES);
            }

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateTopicRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _topicRepository.UpdateTopic(request);

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
            var result = await _topicRepository.DeleteTopic(id);

            if (result.IsSuccess)
            {
                _memoryCache.Remove(CACHE_KEY_GET_CATEGORIES);
            }

            return Ok(result);
        }
    }
}