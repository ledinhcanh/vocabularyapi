using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using API.Models.AppConfig;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/cache")]
    [ApiController]
    [Authorize]
    public class CacheController : ControllerBase
    {
        private readonly ILogger<CacheController> _logger;
        private readonly IMemoryCache _memoryCache;

        public CacheController(ILogger<CacheController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
        }
        [HttpGet("clear-cache")]
        public async Task<IActionResult> ClearCache()
        {
            var CacheKey = "api/cache/clear-cache";
            _logger.Log(LogLevel.Information, CacheKey); 
            if (_memoryCache is MemoryCache cache)
            {
                cache.Clear();
            }
            return Ok(new AppResponse<object>()
            {
                Data = "Ok",
                IsSuccess = true,
                Message = "Thành công"
            });
        }
    }
}
