using Microsoft.EntityFrameworkCore;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Handle;
using API.Services.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using API.Models.AppSettingModels;
using System.Text.Json;

namespace API.Services.Implement
{
    public class BannerFooterRepository : IBannerFooterRepository
    {
        private readonly GovHniContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _siteId;
        private readonly string? _bannerLibraryItemKey;
        private readonly string? _webDomain;
        public BannerFooterRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            this._configuration = configuration;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._bannerLibraryItemKey = configuration.GetValue<string>("AppSettings:BannerLibraryItemKey");
        }

        public async Task<AppResponse<object>> GetBanner()
        {
            var Banner = await _context.Banners.Where(x =>
               x.SiteId == _siteId
               && x.BannerType == 0
               && x.Approved == true
               && x.EndDate >= DateTime.Now.Date
               && x.StartDate <= DateTime.Now.Date
               ).OrderByDescending(x => x.IsDefault).Select(x => new
               {
                   x.BannerTitle,
                   x.BannerContent
               }).FirstOrDefaultAsync();
            if (Banner == null)
            {
                return new AppResponse<object>()
                {
                    Data = null,
                    Message = "Không có dữ liệu",
                    IsSuccess = false
                };
            }
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = new
                {
                    Banner.BannerTitle,
                    BannerContent = Banner.BannerContent?.IncludeDomainToDetail(_webDomain,
                               new Dictionary<string, string>() {
                                { "a", "href" },
                                { "img", "src" },
                                { "video", "src" },
                                { "source", "src" },
                               }, false)
                }
            };
        }

        public async Task<AppResponse<object>> GetDefaultImagesBanner()
        {
            var setting = await _context.AppMobileSettings.FindAsync(this._bannerLibraryItemKey, this._siteId);
            if (setting == null || string.IsNullOrEmpty(setting.KeySettingValue)) return new AppResponse<object>() { Data = null, IsSuccess = false, Message = "Không có dữ liệu" };
            var SettingId = JsonSerializer.Deserialize<int>(setting.KeySettingValue);
            var LibraryItemBanner = await _context.LibraryItems.Where(x => x.ItemId == SettingId && x.SiteId == _siteId && x.Approved == true).Select(x => new
            {
                x.Title
            }).FirstOrDefaultAsync();
            if (LibraryItemBanner == null)
            {
                return new AppResponse<object>()
                {
                    Data = null,
                    Message = "Không có dữ liệu",
                    IsSuccess = false
                };
            }
            var Files = await _context.LibraryItemFiles.Where(x => x.ItemId == SettingId).Select(x => new
            {
                x.Fid,
                x.Title,
                x.Description,
                x.OriginalImage,
                x.VirtualFilePath
            }).ToListAsync();

            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = new
                {
                    LibraryItem = LibraryItemBanner,
                    LibraryItemFiles = Files?.Select(x => new
                    {
                        x.Fid,
                        x.Title,
                        x.Description,
                        FilePath = (x.VirtualFilePath?.Replace("~", string.Empty) + "/" + x.OriginalImage).AddFirstHostUrl(_webDomain)
                    })
                }
            };
        }

        public async Task<AppResponse<object>> GetFooter()
        {
            var Footer = await _context.Banners.Where(x =>
                x.SiteId == _siteId
                && x.BannerType == 1
                && x.Approved == true
                && x.EndDate >= DateTime.Now.Date
                && x.StartDate <= DateTime.Now.Date
                ).OrderByDescending(x => x.IsDefault).Select(x => new
                {
                    x.BannerTitle,
                    x.BannerContent
                }).FirstOrDefaultAsync();
            if (Footer == null)
            {
                return new AppResponse<object>()
                {
                    Data = null,
                    Message = "Không có dữ liệu",
                    IsSuccess = false
                };
            }
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = new
                {
                    Footer.BannerTitle,
                    BannerContent = Footer.BannerContent?.IncludeDomainToDetail(_webDomain,
                               new Dictionary<string, string>() {
                                { "a", "href" },
                                { "img", "src" },
                                { "video", "src" },
                                { "source", "src" },
                               }, true)
                }
            };
        }
    }
}
