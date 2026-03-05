using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Models.Request.Authentication;
using API.Services.Interface;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController] 
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthenticationRepository authenticationRepository, ILogger<AuthController> logger)
        {
            this._authenticationRepository = authenticationRepository;
            this._logger = logger;
        }
        [HttpPost("login")]
        public async Task<IActionResult> GetToken([FromBody] UserTokenRequest request)
        {
            this._logger.Log(LogLevel.Information, "Authentication " + request.Username);
            return Ok(await _authenticationRepository.GetToken(request));
        }
    }
}
