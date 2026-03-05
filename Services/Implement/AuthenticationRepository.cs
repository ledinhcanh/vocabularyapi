using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Request.Authentication;
using API.Models.Response.Authentication;
using API.Services.Interface;
using System.Linq;

namespace API.Services.Implement
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ApiDBContext _context;
        public AuthenticationRepository(IConfiguration configuration, ApiDBContext context)
        {
            _context = context;
            _configuration = configuration;

        }
        public async Task<AppResponse<UserTokenResponse>> GetToken(UserTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return new AppResponse<UserTokenResponse>
                {
                    Message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu",
                    IsSuccess = false
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AppResponse<UserTokenResponse>
                {
                    Message = "Tài khoản hoặc mật khẩu không chính xác",
                    IsSuccess = false
                };
            }

            if (user.IsActive == false)
            {
                return new AppResponse<UserTokenResponse>
                {
                    Message = "Tài khoản của bạn đã bị khóa",
                    IsSuccess = false
                };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username), 
            new Claim(ClaimTypes.Role, user.Role)
             };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AppResponse<UserTokenResponse>
            {
                Data = new UserTokenResponse
                {
                    AccessToken = tokenHandler.WriteToken(token),
                    ValidFrom = token.ValidFrom.ToLocalTime(),
                    ValidTo = token.ValidTo.ToLocalTime(),
                    Role = user.Role, 
                    FullName = user.FullName 
                },
                Message = "Đăng nhập thành công",
                IsSuccess = true
            };
        }

    }
}
