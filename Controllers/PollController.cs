using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.Request.DraftDocument;
using API.Models.Request.Poll;
using API.Services.Implement;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/poll")]
    [ApiController]
    [Authorize]
    public class PollController : AppControllerBase
    {
        private readonly IPollRepository _repository;
        private readonly ILogger<PollController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        public PollController(IPollRepository repository, ILogger<PollController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._repository = repository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }
        [HttpGet("get-default-poll")]
        public async Task<IActionResult> GetDefaultPoll()
        {
            var CacheKey = "api/portal/get-default-poll";
            _logger.Log(LogLevel.Information, CacheKey);
            if (_enableCache && !ClearCache() && _memoryCache.TryGetValue(CacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Add("Cache", "true");
                return Ok(result);
            }
            var Data = await _repository.GetDefaultPoll();
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
        [HttpPost("post-poll-vote")]
        public async Task<IActionResult> PostPollVote(PollVoteRequest request)
        {
            var CacheKey = "api/poll/post-poll-vote: " + JsonSerializer.Serialize(request);
            _logger.Log(LogLevel.Information, CacheKey);
            var Data = await _repository.PostPollVote(request);
            return Ok(Data);
        }
    }
}
