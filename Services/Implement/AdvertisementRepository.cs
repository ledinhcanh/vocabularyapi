using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly GovHniContext _context;
        private readonly int _siteId;
        private readonly int _advertisementModuleId;
        private readonly int _multiFunctionControlId;
        private readonly string? _webDomain;
        public AdvertisementRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext; 
            this._advertisementModuleId = configuration.GetValue<int>("AppSettings:Advertisement:ModuleId");
            this._multiFunctionControlId = configuration.GetValue<int>("AppSettings:MultiFunctionControl:ModuleId");
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out _siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
        }

        public async Task<AppResponse<object>> GetAdvertisementByPage(int pageId)
        {
            var UIPanelID = await _context.Pages.Where(x => x.PageId == pageId && x.SiteId == _siteId).Select(x => x.UipanelId).FirstOrDefaultAsync();
            if (UIPanelID == null)
            {
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy dữ liệu"
                };
            }
            var IdModules = new[] { _advertisementModuleId, _multiFunctionControlId }.ToList();
            var PageControls = await (from pc in _context.PageControls
                                      join pl in _context.PageLayouts on pc.PageLayoutId equals pl.PageLayoutId
                                      where pc.PageId == pageId
                                      && IdModules.Contains(pc.ModuleId)
                                      && pl.UipanelId == UIPanelID
                                      && pc.StateValue != null
                                      orderby pl.Panel, pl.OrderInPanel, pc.ZoneId, pc.OrderInZone
                                      select new PageControl()
                                      {
                                          PageControl1 = pc.PageControl1,
                                          ModuleId = pc.ModuleId,
                                          StateValue = pc.StateValue
                                      }).ToListAsync();
            //var PageControls = await _context.PageControls.Where(x => x.PageId == pageId && IdModules.Contains(x.ModuleId)).Select(x => new PageControl()
            //{
            //    PageControl1 = x.PageControl1,
            //    ModuleId = x.ModuleId,
            //    StateValue = x.StateValue
            //}).ToListAsync();
            if (PageControls == null || PageControls.Count == 0) return new AppResponse<object>()
            {
                IsSuccess = false,
                Message = "Không tìm thấy dữ liệu"
            };
            var AdsStatement = PageControls.Where(x => x.ModuleId == _advertisementModuleId && !string.IsNullOrEmpty(x.StateValue) && !string.IsNullOrWhiteSpace(x.StateValue)).Select(x =>
            {
                var xml = x.StateValue?.ToXmlDocument();
                int AdId = -1;
                if (!(xml != null && xml.DocumentElement?.Attributes["AdvertID"] != null && int.TryParse(xml.DocumentElement.Attributes["AdvertID"]?.Value, out AdId)))
                    AdId = -1;
                return new
                {
                    x.ModuleId,
                    AdId = AdId,
                    x.PageControl1
                };
            }).ToList();
            var AdsStatementIds = AdsStatement.Where(x => x.AdId > 0).Select(x => x.AdId).Distinct().ToList();
            if (AdsStatementIds != null && AdsStatementIds.Count > 0)
            {
                var Ads = await _context.Advertisements.Where(x =>
                            AdsStatementIds.Contains(x.AdvId)
                            && x.SiteId == _siteId
                            && x.FromDate <= DateTime.Now.Date
                            && x.ToDate >= DateTime.Now.Date
                            && x.Approved == true).Select(x => new
                            {
                                x.AdvId,
                                x.AdvContent
                            }).ToListAsync();

                var AdValues = AdsStatement.Select(x => new
                {
                    x.PageControl1,
                    x.ModuleId,
                    StateValue = Ads.Where(t => t.AdvId == x.AdId).Select(t => t.AdvContent).FirstOrDefault()
                }).ToList();
                PageControls.ForEach(e =>
                {
                    if (e.ModuleId == _advertisementModuleId)
                    {
                        var valueAd = AdValues.Find(x => x.PageControl1 == e.PageControl1);
                        if (valueAd != null)
                        {
                            e.StateValue = valueAd.StateValue;
                        }
                    }
                });
            }
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = PageControls.Select(x => new
                {
                    PageControlId = x.PageControl1,
                    x.ModuleId,
                    StateValue = x.StateValue?.IncludeDomainToDetail(_webDomain,
                               new Dictionary<string, string>() {
                                { "a", "href" },
                                { "img", "src" },
                                { "video", "src" },
                                { "source", "src" },
                               }, true)
                }).ToList()
            };
        }
    }
}
