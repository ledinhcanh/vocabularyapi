using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.Request.Question;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/question-content")]
    [ApiController]
    [Authorize]
    public class QuestionContentController : AppControllerBase
    {
        private readonly IQuestionContentRepository _reposioty;
        private readonly ILogger<QuestionContentController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public QuestionContentController(IQuestionContentRepository reposioty, ILogger<QuestionContentController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._reposioty = reposioty;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpGet("get-question-categories")]
        public async Task<IActionResult> GetCommonCatalog()
        {
            var CacheKey = "api/question-content/get-question-categories";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _reposioty.GetCategories();
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
        [HttpPost("get-question-content-list")]
        public async Task<IActionResult> GetListQuestions([FromBody] GetQuestionRequest request)
        {
            var CacheKey = "api/question-content/get-question-content-list: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppPagingResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _reposioty.GetListQuestions(request);
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
        [HttpPost("get-question-content-detail/{questionid}")]
        public async Task<IActionResult> GetQuestionDetail(int questionid)
        {
            var CacheKey = "api/question-content/get-question-detail: " + questionid;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _reposioty.GetQuestionDetail(questionid);
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
        [HttpPost("push-question-content")]
        public async Task<IActionResult> PushQuestionContent([FromBody] PushQuestionContentRequest request)
        {
            var CacheKey = "api/question-content/push-question-content: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            var Data = await _reposioty.PushQuestionContent(request);
            return Ok(Data);
        }
        [HttpPost("vote-question-content")]
        public async Task<IActionResult> VoteQuestionContent([FromBody] VoteQuestionContentRequest request)
        {
            var CacheKey = "api/question-content/vote-question-content: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            var Data = await _reposioty.VoteQuestionContent(request);
            return Ok(Data);
        }
    }
}
