using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Linq.Expressions;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Request.BaseRequest;
using API.Models.Request.LibraryItem;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class LibraryItemRepository : ILibraryItemRepository
    {

        private readonly GovHniContext _context;
        private readonly int _siteId;
        private readonly string? _webDomain;
        private readonly string? _libraryItemDefaultSettingKey;
        private readonly IArticleRepository _articleRepository;
        public LibraryItemRepository(GovHniContext GovHniContext, IHttpContextAccessor httpContextAccessor, IArticleRepository articleRepository, IConfiguration configuration)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._articleRepository = articleRepository;
            this._libraryItemDefaultSettingKey = configuration.GetValue<string>("AppSettings:LibraryItemDefaultSettingKey");
        }

        public async Task<AppResponse<object>> GetDefaultItemDetail()
        {
            if (string.IsNullOrEmpty(_libraryItemDefaultSettingKey))
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            var Setting = await _context.AppMobileSettings.FindAsync(_libraryItemDefaultSettingKey, _siteId);
            if (Setting == null || string.IsNullOrEmpty(Setting.KeySettingValue) || !Setting.KeySettingValue.IsInteger())
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            int ItemId = int.Parse(Setting.KeySettingValue);
            return await GetLibraryItemDetail(ItemId);
        }

        public async Task<AppResponse<object>> GetLibraryItemDetail(int itemId)
        {
            var Item = await _context.LibraryItems.Where(x => x.SiteId == _siteId && x.Approved == true && x.IsPrivate != true && x.ItemId == itemId).Select(x => new
            {
                x.ItemId,
                x.Title,
                x.ImagePath,
                x.ItemType,
                x.DateCreate,
                x.IsNew,
                x.IsHot,
                x.ContentList,
                x.Counter
            }).FirstOrDefaultAsync();
            if (Item == null)
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            var Files = await _context.LibraryItemFiles.Where(x => x.ItemId == itemId).OrderBy(x => x.OrderInList).ThenBy(x => x.Fid).Select(x => new
            {
                x.Fid,
                x.Title,
                x.Description,
                x.VirtualFilePath,
                x.OriginalImage,
            }).ToListAsync();
            var result = new
            {
                Item.ItemId,
                Item.Title,
                ImagePath = Item.ImagePath.AddFirstHostUrl(_webDomain),
                Item.ItemType,
                Item.DateCreate,
                Item.IsNew,
                Item.IsHot,
                Item.ContentList,
                Item.Counter,
                Files = Files?.Select(x => new
                {
                    x.Fid,
                    x.Title,
                    x.Description,
                    Filepath = (x.VirtualFilePath?.TrimEnd('/') + "/" + x.OriginalImage?.TrimEnd('/')).AddFirstHostUrl(_webDomain)
                })
            };
            return new AppResponse<object>
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = result
            };
        }

        public async Task<AppResponse<object>> GetLibraryItemRelated(int itemId)
        {
            var DetailItemType = await _context.LibraryItems.Where(x => x.SiteId == _siteId && x.Approved == true && x.IsPrivate != true && x.ItemId == itemId).Select(x => new
            {
                x.ItemId,
                x.ItemType,
            }).FirstOrDefaultAsync();
            if (DetailItemType == null)
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            Expression<Func<LibraryItem, bool>> expression = x => x.SiteId == _siteId && x.Approved == true && x.IsPrivate != true && x.ItemId != itemId;
            var Count = await _context.LibraryItems.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            if (!string.IsNullOrEmpty(DetailItemType.ItemType))
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.ItemType == DetailItemType.ItemType
                );
            }
            int PageSize = 10;
            var _pagingSetting = await _articleRepository.GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) PageSize = _pagingSetting.PageSize;
            var Data = await _context.LibraryItems.Where(expression).OrderByDescending(x => x.DateCreate).Take(PageSize).Select(x => new
            {
                x.ItemId,
                x.Title,
                x.ImagePath,
                x.ItemType,
                x.DateCreate,
                x.IsNew,
                x.IsHot
            }).ToListAsync();
            var result = Data.Select(x => new
            {
                x.ItemId,
                x.Title,
                ImagePath = x.ImagePath.AddFirstHostUrl(_webDomain),
                x.ItemType,
                x.DateCreate,
                x.IsNew,
                x.IsHot
            }).ToList();
            return new AppResponse<object>
            {
                Data = result,
                IsSuccess = true,
                Message = "Thành công"
            };
        }

        public async Task<AppPagingResponse<object>> GetLibraryItems(RequestGetLibraryItem request)
        {
            Expression<Func<LibraryItem, bool>> expression = x => x.SiteId == _siteId && x.Approved == true && x.IsPrivate != true;
            var Count = await _context.LibraryItems.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            if (request.ItemType != null && request.ItemType.Count > 0)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.ItemType != null && request.ItemType.Contains(e.ItemType)
                );
            }
            int PageSize = 10;
            var _pagingSetting = await _articleRepository.GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) PageSize = _pagingSetting.PageSize;
            var Data = await _context.LibraryItems.Where(expression).OrderByDescending(x => x.DateCreate).Skip(request.PageIndex * PageSize).Take(PageSize).Select(x => new
            {
                x.ItemId,
                x.Title,
                x.ImagePath,
                x.ItemType,
                x.DateCreate,
                x.IsNew,
                x.IsHot
            }).ToListAsync();
            var result = Data.Select(x => new
            {
                x.ItemId,
                x.Title,
                ImagePath = x.ImagePath.AddFirstHostUrl(_webDomain),
                x.ItemType,
                x.DateCreate,
                x.IsNew,
                x.IsHot
            }).ToList();
            return new AppPagingResponse<object>
            {
                Data = result,
                IsSuccess = true,
                Message = "Thành công",
                Paging = new PagingResponse()
                {
                    CurrentItemCount = Data.Count,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalRows = Count
                }
            };
        }
    }
}
