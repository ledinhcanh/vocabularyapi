using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Context;
using API.Models.Database.Enum;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Request.Document;
using API.Models.Response.Document;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _webDomain;
        private readonly int _siteId;
        private readonly IMapper _mapper;
        private readonly IArticleRepository _articleRepository;
        public DocumentRepository(GovHniContext GovHniContext, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor, IArticleRepository articleRepository)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._mapper = mapper;
            _articleRepository = articleRepository;
        }

        public async Task<AppResponse<object>> GetCommonCatalog()
        {
            var Type = await _context.CommonCatalogs.Where(x => x.CatalogType == (int)CommonCatalogType.Type && x.SiteId == _siteId).OrderBy(x => x.OrderInList).ThenBy(x => x.CatalogName).Select(x => new
            {
                x.CatalogId,
                x.CatalogName,
            }).ToListAsync();
            var AgencyIssued = await _context.CommonCatalogs.Where(x => x.CatalogType == (int)CommonCatalogType.AgencyIssued && x.SiteId == _siteId).OrderBy(x => x.OrderInList).ThenBy(x => x.CatalogName).Select(x => new
            {
                x.CatalogId,
                x.CatalogName,
            }).ToListAsync();
            var Field = await _context.CommonCatalogs.Where(x => x.CatalogType == (int)CommonCatalogType.Field && x.SiteId == _siteId).OrderBy(x => x.OrderInList).ThenBy(x => x.CatalogName).Select(x => new
            {
                x.CatalogId,
                x.CatalogName,
            }).ToListAsync();
            var Years = await _context.SteeringDocuments.Select(x => x.DateOfIssued.Year).Distinct().OrderByDescending(z => z).ToListAsync();
            return new AppResponse<object>()
            {
                Message = "Thành công",
                Data = new
                {
                    CatalogType = Type,
                    CatalogAgencyIssued = AgencyIssued,
                    CatalogField = Field,
                    CatalogYears = Years
                },
                IsSuccess = true
            };
        }

        public async Task<AppResponse<object>> GetDocumentDetail(int DocumentId)
        {
            var Document = await _context.SteeringDocuments.Where(x => x.SteeringId == DocumentId && x.Approved == true && x.SiteId == _siteId).FirstOrDefaultAsync();
            if (Document == null)
            {
                return new AppResponse<object>()
                {
                    Message = "Không tìm thấy dữ liệu",
                    IsSuccess = false
                };
            }
            var result = _mapper.Map<SteeringDocumentResponse>(Document);

            result.FilePaths = await _context.SteeringFiles.Where(x => x.SteeringId == result.SteeringId).Select(x => x.FilePath).ToListAsync();
            if (result.FilePaths != null && result.FilePaths.Count > 0)
            {
                result.FilePaths = result.FilePaths.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.AddFirstHostUrl(_webDomain)).ToList();
            }
            result.FieldName = await _context.CommonCatalogs.Where(x => x.CatalogId == result.FieldId && x.SiteId == _siteId).Select(x => x.CatalogName).FirstOrDefaultAsync();
            result.TypeName = await _context.CommonCatalogs.Where(x => x.CatalogId == result.TypeId && x.SiteId == _siteId).Select(x => x.CatalogName).FirstOrDefaultAsync();
            if (result.AgencyIssued != null) result.AgencyIssuedyName = await _context.CommonCatalogs.Where(x => x.CatalogId == result.AgencyIssued && x.SiteId == _siteId).Select(x => x.CatalogName).FirstOrDefaultAsync();
            result.DocUrl = result.DocUrl?.AddFirstHostUrl(_webDomain);

            return new AppResponse<object>()
            {
                Message = "Thành công",
                Data = result,
                IsSuccess = true
            };
        }

        public async Task<AppResponse<object>> GetDocumentRelated(int DocumentId)
        {
            Expression<Func<SteeringDocument, bool>> expression = x =>
               x.Approved == true
               && x.SteeringId != DocumentId
               && x.SiteId == this._siteId;

            var Count = await _context.SteeringDocuments.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            int PageSize = 10;
            var _pagingSetting = await _articleRepository.GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) PageSize = _pagingSetting.PageSize;
            var Data = await _context.SteeringDocuments.Where(expression).OrderByDescending(x => x.DateOfIssued).Take(PageSize).Select(x => new
            {
                x.SteeringId,
                x.CodeId,
                x.DateOfIssued,
                x.Epitomize,
                x.DocUrl,
                x.DownCount,
                x.ViewCount
            }).ToListAsync();
            var SteeringIds = Data.Select(x => x.SteeringId).ToList();
            var Files = await _context.SteeringFiles.Where(x => SteeringIds.Contains(x.SteeringId)).ToListAsync();
            var result = Data.Select(x => new
            {
                x.SteeringId,
                x.CodeId,
                x.DateOfIssued,
                x.Epitomize,
                DocUrl = x.DocUrl?.AddFirstHostUrl(_webDomain),
                x.DownCount,
                x.ViewCount,
                FilePaths = Files.Where(t => t.SteeringId == x.SteeringId).Select(t => t.FilePath.AddFirstHostUrl(_webDomain))
            }).ToList();
            return new AppResponse<object>
            {
                Data = result,
                IsSuccess = true,
                Message = "Thành công",
            };
        }

        public async Task<AppPagingResponse<object>> GetListDocument(GetDocumentsRequest request)
        {
            Expression<Func<SteeringDocument, bool>> expression = x =>
               x.Approved == true
               && x.SiteId == this._siteId;

            if (!string.IsNullOrEmpty(request.KeySearch) || !string.IsNullOrWhiteSpace(request.KeySearch))
            {
                request.KeySearch = request.KeySearch.Trim();
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    (e.CodeId != null && e.CodeId.Contains(request.KeySearch))
                    ||
                    (e.Epitomize != null && e.Epitomize.Contains(request.KeySearch))
                );
            }
            if (request.TypeID != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.TypeId == request.TypeID
                );
            }
            if (request.FieldID != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.FieldId.Contains(request.FieldID.Value.ToString())
                );
            }
            if (request.AgencyIssued != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.AgencyIssued == request.AgencyIssued
                );
            }
            if (request.Year != null)
            {
                DateTime From = new DateTime(request.Year.Value, 1, 1);
                DateTime To = new DateTime(request.Year.Value + 1, 1, 1);
                expression = ExpressionCustoms.AndAlso(expression, e =>
                    e.DateOfIssued >= From
                    && e.DateOfIssued < To
                );
            }
            var Count = await _context.SteeringDocuments.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            int PageSize = 10;
            var _pagingSetting = await _articleRepository.GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) PageSize = _pagingSetting.PageSize;
            var Data = await _context.SteeringDocuments.Where(expression).OrderByDescending(x => x.DateOfIssued).Skip(request.PageIndex * PageSize).Take(PageSize).Select(x => new
            {
                x.SteeringId,
                x.CodeId,
                x.DateOfIssued,
                x.Epitomize,
                x.DocUrl,
                x.DownCount,
                x.ViewCount
            }).ToListAsync();
            var SteeringIds = Data.Select(x => x.SteeringId).ToList();
            var Files = await _context.SteeringFiles.Where(x => SteeringIds.Contains(x.SteeringId)).ToListAsync();
            var result = Data.Select(x => new
            {
                x.SteeringId,
                x.CodeId,
                x.DateOfIssued,
                x.Epitomize,
                DocUrl = x.DocUrl?.AddFirstHostUrl(_webDomain),
                x.DownCount,
                x.ViewCount,
                FilePaths = Files.Where(t => t.SteeringId == x.SteeringId).Select(t => t.FilePath.AddFirstHostUrl(_webDomain))
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
