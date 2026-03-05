using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text.RegularExpressions;
using API.Models.AppConfig;
using API.Models.AppSettingModels;
using API.Models.Database.Context;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class PortalRepository : IPortalRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _groupSiteSettingKey;
        private readonly int _siteId;
        public PortalRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            this._groupSiteSettingKey = configuration.GetValue<string>("AppSettings:GroupSiteSettingKey");
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
        }

        public async Task<AppResponse<object>> GetPortalList()
        {
            var GrSite = await _context.AppMobileSettings.FindAsync(this._groupSiteSettingKey, this._siteId);
            if (GrSite == null || string.IsNullOrEmpty(GrSite.KeySettingValue)) return new AppResponse<object>() { Data = null, IsSuccess = false, Message = "Không có dữ liệu" };
            var _groupSiteIds = JsonSerializer.Deserialize<GroupSiteSetting>(GrSite.KeySettingValue);
            if (_groupSiteIds == null || _groupSiteIds.GroupSite == null || _groupSiteIds.GroupSite.DisplayIDs == null) return new AppResponse<object>() { Data = null, IsSuccess = false, Message = "Không có dữ liệu" };

            var query = from gr in _context.GroupSites
                        join s in _context.Sites on gr.GroupSiteId equals s.GroupSiteId
                        where _groupSiteIds.GroupSite.DisplayIDs.Contains(gr.GroupSiteId)
                        group s by new { gr.GroupSiteName, gr.OrderInList } into gGroup
                        select new
                        {
                            GroupSite = gGroup.Key,
                            Data = gGroup.Select(t => new
                            {
                                t.FullSiteName,
                                t.SiteId,
                                t.FirstHostUrl,
                                t.SecondHostUrl,
                                t.OrderInList
                            }).ToList()
                        };
            var Data = await (query).OrderBy(x => x.GroupSite.OrderInList).ToListAsync();
            return new AppResponse<object>()
            {
                Data = Data.Select(x => new
                {
                    x.GroupSite,
                    Data = x.Data.OrderBy(t => t.OrderInList).ThenBy(t => t.FullSiteName).ToList()
                }),
                IsSuccess = true,
                Message = "Thành công"
            };
        }
    }
}
