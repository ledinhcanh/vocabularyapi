using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class WebLinkRepository : IWebLinkRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _webDomain;
        private readonly int _siteId;
        public WebLinkRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
        }
        public async Task<AppResponse<object>> GetListWebLink()
        {
            var Links = await _context.WebLinks.Where(x => x.SiteId == _siteId).OrderBy(x => x.LinkOrder).Select(x => new
            {
                x.LinkId,
                x.Title,
                x.Url,
                x.ImagePath,
                x.LinkOrder
            }).ToListAsync();
            return new AppResponse<object>()
            {
                Data = Links,
                IsSuccess = true,
                Message = "Thành công"
            };
        }
    }
}
