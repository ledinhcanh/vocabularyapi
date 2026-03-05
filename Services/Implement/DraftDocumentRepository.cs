using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Request.DraftDocument;
using API.Models.Response.DraftDocumentsRequest;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class DraftDocumentRepository : IDraftDocumentRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _webDomain;
        private readonly int _siteId;
        private readonly IMapper _mapper;
        public DraftDocumentRepository(GovHniContext GovHniContext, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._mapper = mapper;
        }

        public async Task<AppResponse<object>> CommentDraftDocument(DraftDocumentCommentRequest request)
        {
            var ExistInDB = await _context.VanBanLayYKiens.Where(x => x.VanBanId == request.DraftDocumentId && x.Siteid == _siteId).FirstOrDefaultAsync();
            if (ExistInDB == null)
            {
                return new AppResponse<object>()
                {
                    Message = "Không tìm thấy dữ liệu",
                    IsSuccess = false
                };
            }
            else if (ExistInDB.NgayKetThuc != null && ExistInDB.NgayKetThuc.Value.Date < DateTime.Now.Date)
            {
                return new AppResponse<object>()
                {
                    Message = "Văn bản đã hết hạn lấy góp ý",
                    IsSuccess = false
                };
            }
            YKienGopY gopy = new YKienGopY()
            {
                CreateDate = DateTime.Now,
                DiaChi = request.Address,
                Email = request.Email,
                HoTen = request.Name,
                InsertDate = DateTime.Now,
                IsPublic = false,
                NoiDung = request.Content,
                TieuDe = request.Title,
                VanBanId = request.DraftDocumentId
            };
            await _context.YKienGopies.AddAsync(gopy);
            await _context.SaveChangesAsync();
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = gopy.Id
            };

        }

        public async Task<AppResponse<object>> GetDraftDocumentDetail(int DraftDocumentId)
        {
            var data = await _context.VanBanLayYKiens.Where(x => x.VanBanId == DraftDocumentId && x.Siteid == _siteId).FirstOrDefaultAsync();
            if (data == null)
            {
                return new AppResponse<object>()
                {
                    Message = "Không tìm thấy dữ liệu",
                    IsSuccess = false
                };
            }
            var result = _mapper.Map<DraftDocumentResponse>(data);

            result.FilePaths = await _context.VanBanLayYKienFiles.Where(x => x.VanBanId == result.VanBanId).Select(x => x.FilePath).ToListAsync();
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

        public async Task<AppPagingResponse<object>> GetListDraftDocument(GetDraftDocumentRequest request)
        {
            Expression<Func<VanBanLayYKien, bool>> expression = x =>
                 x.Siteid == this._siteId;
            if (!string.IsNullOrEmpty(request.KeySearch) || !string.IsNullOrWhiteSpace(request.KeySearch))
            {
                request.KeySearch = request.KeySearch.Trim();
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    (e.TieuDe != null && e.TieuDe.Contains(request.KeySearch))
                );
            }
            if (request.IsExpired)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.NgayKetThuc < DateTime.Now.Date
                );
            }
            else if (!request.IsExpired)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.NgayKetThuc >= DateTime.Now.Date
                );
            }
            var Count = await _context.VanBanLayYKiens.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }

            var Data = await _context.VanBanLayYKiens.Where(expression).OrderByDescending(x => x.NgayKetThuc).ThenByDescending(x => x.CreateDate).Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(x => new
            {
                x.VanBanId,
                x.FilePath,
                x.TrichYeu,
                x.NgayBatDau,
                x.NgayKetThuc,
                x.TieuDe
            }).ToListAsync();
            var result = Data.Select(x => new
            {
                x.VanBanId,
                x.TieuDe,
                FilePath = x.FilePath?.AddFirstHostUrl(_webDomain),
                x.TrichYeu,
                x.NgayBatDau,
                x.NgayKetThuc,
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
