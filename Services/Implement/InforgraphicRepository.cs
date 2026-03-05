using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Response.Article;
using API.Models.Response.Inforgraphic;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class InforgraphicRepository : IInforgraphicRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _webDomain;
        private readonly int _siteId;
        private readonly string? _infographicSettingKey;
        public InforgraphicRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._infographicSettingKey = configuration.GetValue<string>("AppSettings:InfographicSettingKey");
        }

        public async Task<AppResponse<object>> GetInforgraphics()
        {
            if (string.IsNullOrEmpty(this._infographicSettingKey))
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Key chưa được cấu hình"
                };
            }
            var setting = await _context.AppMobileSettings.FindAsync(_infographicSettingKey, _siteId);
            if (setting == null || string.IsNullOrEmpty(setting.KeySettingValue))
            {
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy thông tin key"
                };
            }
            List<InforgraphicSetting> inforgraphicSettings = JsonSerializer.Deserialize<List<InforgraphicSetting>>(setting.KeySettingValue);
            if (inforgraphicSettings == null || inforgraphicSettings.Count == 0)
            {
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Key value json cannot be parse to object"
                };
            }
            foreach (var item in inforgraphicSettings)
            {
                if (item.Count <= 0) continue;
                item.Articles = await _context.VwArticleLists.Where(x => x.ArticleCatId == item.ArticleCatId && x.LanguageId == item.LanguageId).OrderByDescending(x => x.DateCreate).Take(item.Count).Select(x => new ArticleResponse()
                {
                    ArticleCatId = x.ArticleCatId,
                    ArticleId = x.ArticleId,
                    Title = x.Title,
                    ImagePath = x.ImagePath,
                    DateCreate = x.DateCreate,
                    Summary = x.Summary,
                    Author = x.Author,
                    IsNew = x.IsNew,
                    IsHot = x.IsHot,
                    DateApprove = x.DateApprove
                }).ToListAsync();
                if (item.Articles != null)
                {
                    item.Articles.ForEach(x =>
                    {
                        x.ImagePath = x.ImagePath.AddFirstHostUrl(_webDomain);
                        x.ShareUrl = (x.CategorySeo + "/" + x.Title.VnTextToRequestText() + "-" + x.ArticleId)?.AddFirstHostUrl(_webDomain);
                    });
                }
            }
            return new AppResponse<object>()
            {
                Data = inforgraphicSettings,
                IsSuccess = true,
                Message = "Thành công"
            };
        }
    }
}
