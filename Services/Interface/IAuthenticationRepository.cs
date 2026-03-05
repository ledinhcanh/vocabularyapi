using API.Models.AppConfig;
using API.Models.Request.Authentication;
using API.Models.Response.Authentication;

namespace API.Services.Interface
{
    public interface IAuthenticationRepository
    {
        public Task<AppResponse<UserTokenResponse>> GetToken(UserTokenRequest request);
    }
}
