using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.Request.DraftDocument;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/draft-document")]
    [ApiController]
    [Authorize]
    public class DraftDocumentController : AppControllerBase
    {
        private readonly IDraftDocumentRepository _repository;
        private readonly ILogger<DraftDocumentController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;
        public DraftDocumentController(IDraftDocumentRepository repository, ILogger<DraftDocumentController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._repository = repository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }
        [HttpPost("get-list-draft-document")]
        public async Task<IActionResult> GetListDraftDocument(GetDraftDocumentRequest request)
        {
            var CacheKey = "api/draft-document/get-draft-document-: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppPagingResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _repository.GetListDraftDocument(request);
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

        [HttpGet("get-draft-document-detail/{id}")]
        public async Task<IActionResult> GetDraftDocumentDetail(int id)
        {
            var CacheKey = "api/draft-document/get-draft-document-detail: " + id;
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _repository.GetDraftDocumentDetail(id);
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
        [HttpPost("comment-draft-document")]
        public async Task<IActionResult> CommentDraftDocument(DraftDocumentCommentRequest request)
        {
            var CacheKey = "api/draft-document/comment-draft-document: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            var Data = await _repository.CommentDraftDocument(request);           
            return Ok(Data);
        }
    }
}
