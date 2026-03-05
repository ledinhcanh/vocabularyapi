using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Request.Question;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class QuestionContentRepository : IQuestionContentRepository
    {
        private readonly GovHniContext _context;
        private readonly string? _webDomain;
        private readonly int _siteId;
        private readonly IMapper _mapper;
        private readonly IArticleRepository _articleRepository;
        public QuestionContentRepository(GovHniContext GovHniContext, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor, IArticleRepository articleRepository)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._mapper = mapper;
            _articleRepository = articleRepository;
        }

        public async Task<AppResponse<object>> GetCategories()
        {
            var Cats = await _context.QuestionCategories.Where(x => x.SiteId == _siteId).ToListAsync();
            var Data = Cats.GenerateTree(x => x.QuestionCatId, x => x.ParentId);
            if (Data == null || !Data.Any()) return new AppResponse<object>()
            {
                Message = "Không có dữ liệu",
                IsSuccess = false,
                Data = null
            };
            return new AppResponse<object> { Data = Data, IsSuccess = true, Message = "Thành công" };
        }

        public async Task<AppPagingResponse<object>> GetListQuestions(GetQuestionRequest request)
        {

            var _pagingSetting = await _articleRepository.GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) request.PageSize = _pagingSetting.PageSize;

            Expression<Func<QuestionContent, bool>> expression = x =>
              x.Approved == true
              && x.SiteId == this._siteId;
            if (!string.IsNullOrEmpty(request.KeySearch))
            {
                request.KeySearch = request.KeySearch.Trim();
                expression = ExpressionCustoms.AndAlso(expression, e =>
                e.QuestionTitle != null && e.QuestionTitle.Contains(request.KeySearch)
               );
            }
            var Count = await _context.QuestionContents.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            var Data = await _context.QuestionContents.Where(expression).OrderByDescending(x => x.DateResponse).ThenByDescending(x => x.DateCreated).Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(x => new
            {
                x.QuestionId,
                x.QuestionCatId,
                x.QuestionTitle,
                x.CustomerName,
                DateRespone = x.DateResponse,
                x.DateCreated,
                x.Liked,
                x.Disliked
            }).ToListAsync();
            return new AppPagingResponse<object>
            {
                Data = Data,
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

        public async Task<AppResponse<object>> GetQuestionDetail(int QuestionId)
        {
            var question = await _context.QuestionContents.Where(x =>
                x.SiteId == _siteId
                && x.Approved == true
                && x.QuestionId == QuestionId).Select(x => new
                {
                    x.QuestionId,
                    x.QuestionCatId,
                    x.QuestionTitle,
                    x.CustomerName,
                    DateRespone = x.DateResponse,
                    x.DateCreated,
                    x.QuestionContent1,
                    x.AnswerContent,
                    x.FilePath,
                    x.Liked,
                    x.Disliked
                }).FirstOrDefaultAsync();
            if (question == null) return new AppResponse<object>()
            {
                Message = "Không tìm thấy dữ liệu",
                Data = null,
                IsSuccess = false
            };
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Data = new
                {
                    question.QuestionId,
                    question.QuestionCatId,
                    question.QuestionTitle,
                    question.CustomerName,
                    question.DateRespone,
                    question.DateCreated,
                    QuestionContent = HttpUtility.HtmlDecode(question.QuestionContent1),
                    AnswerContent = HttpUtility.HtmlDecode(question.AnswerContent),
                    question.Liked,
                    question.Disliked,
                    FilePath = question.FilePath.AddFirstHostUrl(_webDomain)
                },
                Message = "Thành công"
            };
        }

        public async Task<AppResponse<object>> GetQuestionRelated(int QuestionId)
        {
            int _pageSize = 10;
            var _pagingSetting = await _articleRepository.GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) _pageSize = _pagingSetting.PageSize;

            var questions = await _context.QuestionContents.Where(x =>
               x.SiteId == _siteId
               && x.Approved == true
               && x.QuestionId != QuestionId).OrderByDescending(x => x.DateResponse).ThenByDescending(x => x.DateCreated).Take(_pageSize).Select(x => new
               {
                   x.QuestionId,
                   x.QuestionCatId,
                   x.QuestionTitle,
                   x.CustomerName,
                   DateRespone = x.DateResponse,
                   x.DateCreated,
                   x.Liked,
                   x.Disliked
               }).ToListAsync();
            if (questions == null || questions.Count == 0) return new AppResponse<object>()
            {
                Message = "Không tìm thấy dữ liệu",
                Data = null,
                IsSuccess = false
            };
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Data = questions,
                Message = "Thành công"
            };
        }

        public async Task<AppResponse<object>> PushQuestionContent(PushQuestionContentRequest request)
        {
            var AnyCats = await _context.QuestionCategories.AnyAsync(x => x.SiteId == _siteId && request.QuestionCatId == x.QuestionCatId);
            if (!AnyCats)
            {
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Chuyên mục không cho phép đẩy tin"
                };
            }
            QuestionContent questionContent = new QuestionContent()
            {
                QuestionCatId = request.QuestionCatId,
                SiteId = _siteId,
                Address = request.Address,
                CustomerName = request.CustomerName,
                DateCreated = DateTime.Now,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                QuestionContent1 = request.QuestionContent,
                QuestionTitle = request.QuestionTitle,
                Liked = 0,
                Disliked = 0
            };
            await _context.QuestionContents.AddAsync(questionContent);
            await _context.SaveChangesAsync();
            return new AppResponse<object>()
            {
                Data = questionContent.QuestionId,
                IsSuccess = true,
                Message = "Thành công"
            };
        }

        public async Task<AppResponse<object>> VoteQuestionContent(VoteQuestionContentRequest request)
        {
            var question = await _context.QuestionContents.Where(x =>
                x.SiteId == _siteId
                && x.Approved == true
                && x.QuestionId == request.QuestionId).FirstOrDefaultAsync();
            if (question == null)
            {
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }
            if (request.IsLike)
            {
                question.Liked ??= 0;
                question.Liked++;
            }
            else
            {
                question.Disliked ??= 0;
                question.Disliked++;
            }
            await _context.SaveChangesAsync();
            return new AppResponse<object>()
            {
                Data = question.QuestionId,
                IsSuccess = true,
                Message = "Thành công"
            };
        }
    }
}
