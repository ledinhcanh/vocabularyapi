using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.Database.Identities;
using API.Models.Request.Article;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/article")]
    [ApiController]
    [Authorize]
    public class ArticleController : AppControllerBase
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ILogger<ArticleController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public ArticleController(IArticleRepository articleRepository, ILogger<ArticleController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._articleRepository = articleRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        } 
        [HttpGet("get-hortizontal-categories")]
        public async Task<IActionResult> GetHorizontalCategories()
        {

            var CacheKey = "api/article/get-hortizontal-categories: ";
            _logger.Log(LogLevel.Information, CacheKey);

            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetHorizontalCategories();
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
        [HttpGet("get-vertical-categories")]
        public async Task<IActionResult> GetVerticalCategories()
        {
            var CacheKey = "api/article/get-vertical-categories: ";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetVerticalCategories();
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
        [HttpGet("get-sub-categories/{articleCatId}")]
        public async Task<IActionResult> GetSubCategories(int articleCatId)
        {
            var CacheKey = "api/article/get-sub-categories: " + articleCatId;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetSubCategories(articleCatId);
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
        [HttpPost("get-articles")]
        public async Task<IActionResult> GetArticles([FromBody] GetArticleRequest request)
        {
            var CacheKey = "api/article/get-article-new: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppPagingResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetArticles(request);
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
        [HttpGet("get-single-article-by-catid/{CatId}")]
        public async Task<IActionResult> GetSingleArticleByCatId(int CatId)
        {
            var CacheKey = "api/article/get-single-article-by-catid: " + CatId;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetSingleArticleByCatId(CatId);
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
        [HttpGet("get-detail/{ArticleId}")]
        public async Task<IActionResult> GetArticleDetail(int ArticleId)
        {
            var CacheKey = "api/article/get-article-detail: " + ArticleId;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetArticleDetail(ArticleId);
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
        [HttpPost("get-article-feedback")]
        public async Task<IActionResult> GetArticleFeedback([FromBody] GetArticleFeedbackRequest request)
        {
            var CacheKey = "api/article/get-article-feedback: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetArticleFeedback(request);
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
        [HttpGet("get-related-article")]
        public async Task<IActionResult> GetRelatedArticle([FromQuery] RelatedArticleRequest request)
        {
            var CacheKey = "api/article/get-related-article: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _articleRepository.GetRelatedArticle(request);
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
        [HttpPost("push-article")]
        public async Task<IActionResult> PushArticle([FromBody] PushArticle request)
        {
            var CacheKey = "api/article/PushArticle: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
   
            var Data = await _articleRepository.PushArticle(request);
            return Ok(Data);
        }
    }
}
