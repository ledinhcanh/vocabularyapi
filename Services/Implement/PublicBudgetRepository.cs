using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Request.PublicBudget;
using API.Models.Response.Document;
using API.Models.Response.PublicBudget;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class PublicBudgetRepository : IPublicBudgetRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _webDomain;
        private readonly int _siteId; 
        private readonly IMapper _mapper;
        public PublicBudgetRepository(GovHniContext GovHniContext, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext; 
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.CongKhaiNganSachSiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._mapper = mapper; 
        }
        public async Task<AppResponse<object>> GetCategories()
        {
            var SiteId = _siteId.ToString();
            var data = await _context.CongKhaiNganSachDanhMucs.Where(x => x.SiteId == SiteId).OrderBy(x => x.ThuTu).ToListAsync();
            return new AppResponse<object>()
            {
                Message = "Thành công",
                Data = data,
                IsSuccess = true
            };
        }

        public async Task<AppResponse<object>> GetPublicBudgetDetail(int PublicBudgetId)
        {
            var PublicBudget = await _context.CongKhaiNganSaches.Where(x => x.CongKhaiId == PublicBudgetId && x.SiteId == _siteId).FirstOrDefaultAsync();
            if (PublicBudget == null)
            {
                return new AppResponse<object>()
                {
                    Message = "Không tìm thấy dữ liệu",
                    IsSuccess = false
                };
            }
            var result = _mapper.Map<PublicBudgetResponse>(PublicBudget);
            result.FilePaths = await _context.CongKhaiNganSachFiles.Where(x => x.CongKhaiId == result.CongKhaiId).Select(x => x.FilePath).ToListAsync();
            if (result.FilePaths != null && result.FilePaths.Count > 0)
            {
                result.FilePaths = result.FilePaths.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.AddFirstHostUrl(_webDomain)).ToList();
            }
            result.FilePath = result.FilePath?.AddFirstHostUrl(_webDomain);
            return new AppResponse<object>()
            {
                Message = "Thành công",
                Data = result,
                IsSuccess = true
            };
        }

        public async Task<AppPagingResponse<object>> GetListPublicBudget(GetPublicBudgetRequest request)
        {
            Expression<Func<CongKhaiNganSach, bool>> expression = x =>
                x.SiteId == this._siteId;
            if (!string.IsNullOrEmpty(request.KeySearch) || !string.IsNullOrWhiteSpace(request.KeySearch))
            {
                request.KeySearch = request.KeySearch.Trim();
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    (e.TenBaoCao != null && e.TenBaoCao.Contains(request.KeySearch))
                    ||
                    (e.BieuMau != null && e.BieuMau.Contains(request.KeySearch))
                    ||
                    (e.SoQuyetDinh != null && e.SoQuyetDinh.Contains(request.KeySearch))
                );
            }
            if (!string.IsNullOrEmpty(request.KyBaoCao) || !string.IsNullOrWhiteSpace(request.KyBaoCao))
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    (e.KyBaoCao == request.KeySearch)
                );
            }
            if (request.CategoryId != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.LoaiCongKhai == request.CategoryId
                );
            }
            if (request.PublicDateFrom != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.NgayCongBo >= request.PublicDateFrom.Value.Date
                );
            }
            if (request.PublicDateTo != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.NgayCongBo <= request.PublicDateTo.Value.Date
                );
            }
            var Count = await _context.CongKhaiNganSaches.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }

            var Data = await _context.CongKhaiNganSaches.Where(expression).OrderByDescending(x => x.NgayCongBo).Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(x => new
            {
                x.TenBaoCao,
                x.FilePath,
                x.NgayCongBo,
                x.MoTa,
                x.BieuMau,
                x.SoQuyetDinh,
                x.KyBaoCao,
                x.CongKhaiId
            }).ToListAsync();
            var DataId = Data.Select(x => x.CongKhaiId).Distinct().ToList();
            var Files = await _context.CongKhaiNganSachFiles.Where(x => !string.IsNullOrEmpty(x.FilePath) && DataId.Contains(x.CongKhaiId)).Select(x => new
            {
                x.CongKhaiId,
                x.FilePath
            }).ToListAsync();
            var result = Data.Select(x => new
            {
                x.TenBaoCao,
                FilePath = x.FilePath?.AddFirstHostUrl(_webDomain),
                FileIncludes = Files.Where(t => t.CongKhaiId == x.CongKhaiId).Select(t => t.FilePath.AddFirstHostUrl(_webDomain)).ToList(),
                x.NgayCongBo,
                x.MoTa,
                x.BieuMau,
                x.SoQuyetDinh,
                x.KyBaoCao,
                x.CongKhaiId
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

        public async Task<AppResponse<object>> GetKyBaoCaos()
        {
            var KyBaoCaos = await _context.CongKhaiNganSaches.Where(x => x.SiteId == _siteId).Select(x => x.KyBaoCao).Distinct().ToListAsync();
            if (KyBaoCaos == null || KyBaoCaos.Count == 0)
            {
                return new AppResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            return new AppResponse<object>
            {
                Data = KyBaoCaos.OrderByDescending(x => x).ToList(),
                IsSuccess = true,
                Message = "Thành công",
            };
        }
    }
}
