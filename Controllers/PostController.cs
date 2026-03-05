using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Models.Request.Authentication;
using API.Services.Interface;
using API.Models.Request.Post;
using Microsoft.AspNetCore.Authorization;
using API.Models.AppConfig;
using Microsoft.Extensions.Caching.Memory;
using API.Services.Implement;
using System.Text.Json;

namespace API.Controllers
{
    [Route("api/posts")]
    [ApiController]
    [Authorize]
    public class PostController : AppControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly ILogger<PostController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly bool _enableCache;
        private readonly IConfiguration _configuration;
        private readonly int _timeExpireInMinutes;

        private const string CACHE_KEY_POSTS_PREFIX = "api/posts";
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public PostController(IPostRepository postRepository, ILogger<PostController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._postRepository = postRepository;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
            this._enableCache = configuration.GetValue<bool>("CacheSetting:EnableCache");
            this._timeExpireInMinutes = configuration.GetValue<int>("CacheSetting:TimeExpireInMinutes");
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosts([FromBody] GetPostRequest request)
        {
            var cacheKey = CACHE_KEY_POSTS_PREFIX + JsonSerializer.Serialize(request);

            if (_enableCache && _memoryCache.TryGetValue(cacheKey, out AppPagingResponse<object>? result))
            {
                Response.Headers.Append("Cache", "true");
                return Ok(result);
            }

            var data = await _postRepository.GetPosts(request);

            if (_enableCache)
            {
                _memoryCache.Set(cacheKey, data, TimeSpan.FromMinutes(_timeExpireInMinutes));
            }
            return Ok(data);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetail(int id)
        {
            var cacheKey = $"api/posts/{id}";

            if (_enableCache && _memoryCache.TryGetValue(cacheKey, out AppResponse<object>? result))
            {
                Response.Headers.Append("Cache", "true");
                return Ok(result);
            }

            var data = await _postRepository.GetPostDetail(id);

            if (!data.IsSuccess) return NotFound(data);

            if (_enableCache)
            {
                _memoryCache.Set(cacheKey, data, TimeSpan.FromMinutes(_timeExpireInMinutes));
            }

            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
        {
            var result = await _postRepository.CreatePost(request);

            if (result.IsSuccess)
            {
                if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested && _resetCacheToken.Token.CanBeCanceled)
                {
                    _resetCacheToken.Cancel();
                    _resetCacheToken.Dispose();
                }
                _resetCacheToken = new CancellationTokenSource();
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePostRequest request)
        {
            if (id != request.PostId) return BadRequest("ID không khớp");

            var result = await _postRepository.UpdatePost(request);

            if (result.IsSuccess)
            {
                _memoryCache.Remove($"api/posts/{id}");
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _postRepository.DeletePost(id);

            if (result.IsSuccess)
            {
                _memoryCache.Remove($"api/posts/{id}");
            }

            return Ok(result);
        }

    }
}
