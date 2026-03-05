using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.AppSettingModels;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Models.Request.Article;
using API.Models.Response.Article;
using API.Services.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.IO;
using NAudio.Wave;

namespace API.Services.Implement
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly GovHniContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _siteId;
        private readonly string? _webDomain;
        private readonly string? _pagingSettingKey;
        private readonly string? _hotizontalCategoriesSettingKey;
        private readonly string? _verticalCategoriesSettingKey;
        private readonly string? _categoriesSettingKey;
        public ArticleRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            this._configuration = configuration;
            this._pagingSettingKey = configuration.GetValue<string>("AppSettings:PagingSettingKey");
            this._hotizontalCategoriesSettingKey = configuration.GetValue<string>("AppSettings:HotizontalCategoriesSettingKey");
            this._verticalCategoriesSettingKey = configuration.GetValue<string>("AppSettings:VerticalCategoriesSettingKey");
            this._categoriesSettingKey = configuration.GetValue<string>("AppSettings:ArticleCategoriesSettingKey");
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
        }

        public async Task<PagingSetting?> GetDefaultPagingSetting()
        {
            var stateValue = await _context.AppMobileSettings.FindAsync(_pagingSettingKey, this._siteId);
            if (stateValue != null && stateValue.KeySettingValue != null)
                return JsonConvert.DeserializeObject<PagingSetting>(stateValue.KeySettingValue);
            return null;
        }

        public async Task<AppResponse<object>> GetArticleDetail(int ArticleId)
        {
            var Article = await _context.VwArticleLists.Where(x => x.ArticleId == ArticleId && x.SiteId == _siteId && x.Approved == true).Select(x => new ArticleResponse()
            {
                ArticleCatId = x.ArticleCatId,
                ArticleId = x.ArticleId,
                Title = x.Title,
                Summary = x.Summary,
                ImagePath = x.ImagePath,
                DateCreate = x.DateCreate,
                DateApprove = x.DateApprove,
                Detail = x.Detail,
                Author = x.Author,
                IsHot = x.IsHot,
                IsNew = x.IsNew,
                CategorySeo = x.CategorySeo
            }).FirstOrDefaultAsync();
            if (Article == null) return new AppResponse<object>()
            {
                IsSuccess = false,
                Message = "Không tìm thấy dữ liệu"
            };
            Article.Detail = Article.Detail?.IncludeDomainToDetail(_webDomain,
                               new Dictionary<string, string>() {
                                { "img", "src" },
                                { "video", "src" },
                                { "source", "src" },
                                { "a", "href" },
                               }, false);
            Article.ImagePath = Article.ImagePath?.AddFirstHostUrl(_webDomain);
            Article.ShareUrl = (Article.CategorySeo + "/" + Article.Title.VnTextToRequestText() + "-" + Article.ArticleId)?.AddFirstHostUrl(_webDomain);
            var Audios = await _context.ArticleAudios.Where(x => x.ArticleId == Article.ArticleId).Select(x => new
            {
                x.FilePath,
                x.Region
            }).ToListAsync();
            if (Audios != null)
            {
                Article.Audios = Audios.Select(x => new ArticleAudioResponse()
                {
                    Filepath = x.FilePath.AddFirstHostUrl(_webDomain),
                    Region = x.Region != null && ArticleAudioResponse.VoiceRegion.ContainsKey(x.Region) ? ArticleAudioResponse.VoiceRegion[x.Region] : x.Region
                }).ToList();
            }
            return new AppResponse<object>
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = Article
            };
        }

        public async Task<AppPagingResponse<object>> GetArticleFeedback(GetArticleFeedbackRequest request)
        {
            var articleCatId = await
                (
                    from a in _context.Articles
                    join ac in _context.ArticleCategories on a.ArticleCatId equals ac.ArticleCatId
                    where a.ArticleId == request.ArticleId
                            && a.SiteId == _siteId
                            && a.Approved == true
                            && ac.ShowFeedBack == true
                    select ac.ArticleCatId
                ).AnyAsync();

            if (!articleCatId) return new AppPagingResponse<object>()
            {
                IsSuccess = false,
                Message = "Không có dữ liệu"
            };
            var _pagingSetting = await GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) request.PageSize = _pagingSetting.PageSize;
            Expression<Func<ArticleFeedBack, bool>> expression = x => x.ArticleId == request.ArticleId && x.Approved == true;
            var feedbacks = await _context.ArticleFeedBacks.Where(expression)
                .OrderByDescending(x => x.DateCreate).Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(x => new
                {
                    x.FeedBackId,
                    x.FeedBackTitle,
                    x.FeedBackContent,
                    x.DateCreate,
                    x.FullName
                }).ToListAsync();
            var Count = await _context.ArticleFeedBacks.Where(expression).CountAsync();
            return new AppPagingResponse<object>
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = feedbacks,
                Paging = new PagingResponse()
                {
                    CurrentItemCount = feedbacks.Count,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalRows = Count
                }
            };
        }
        public async Task<AppPagingResponse<object>> GetArticles(GetArticleRequest request)
        {
            Expression<Func<Article, bool>> expression = x =>
                x.Approved == true
                && x.SiteId == this._siteId;
            if (request.IsNew)
            {
                expression = ExpressionCustoms.AndAlso(expression, e => e.IsNew == true);
            }
            if (request.IsHot)
            {
                expression = ExpressionCustoms.AndAlso(expression, e => e.IsHot == true);
            }
            if (request.ArticleCatID != null)
            {
                expression = ExpressionCustoms.AndAlso(expression, e => e.ArticleCatId == request.ArticleCatID);
            }
            if (!string.IsNullOrEmpty(request.KeySearch) || !string.IsNullOrWhiteSpace(request.KeySearch))
            {
                request.KeySearch = request.KeySearch.Trim();
                expression = ExpressionCustoms.AndAlso(expression, e => e.Title != null && e.Title.Contains(request.KeySearch));
            }
            var Count = await _context.Articles.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }
            var _pagingSetting = await GetDefaultPagingSetting();
            if (_pagingSetting != null && _pagingSetting.UseDatabaseSetting) request.PageSize = _pagingSetting.PageSize;
            var Data = await _context.Articles.Where(expression).OrderByDescending(x => x.DateCreate).Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(x => new
            {
                x.Title,
                x.DateApprove,
                x.ArticleId,
                x.Author,
                x.Summary,
                x.IsNew,
                x.IsHot,
                x.ArticleCatId,
                x.ImagePath
            }).ToListAsync();

            return new AppPagingResponse<object>
            {
                Data = Data.Select(x => new
                {
                    x.Title,
                    x.DateApprove,
                    x.ArticleId,
                    x.Author,
                    x.Summary,
                    x.IsNew,
                    x.IsHot,
                    x.ArticleCatId,
                    ImagePath = x.ImagePath.AddFirstHostUrl(_webDomain)
                }),
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

        public async Task<AppConfigResponse<object>> GetArticlesByConfig(AppMobileConfig request)
        {
            if (string.IsNullOrEmpty(request.ConfigValue))
            {
                return new AppConfigResponse<object>()
                {
                    IsSuccess = false,
                    Message = "ConfigValue is null or empty"
                };
            }


            dynamic Config = JsonConvert.DeserializeObject(request.ConfigValue);
            GetArticleRequest model = new GetArticleRequest()
            {
                ArticleCatID = Config?.ArticleCatID,
                PageIndex = Config?.PageIndex,
                PageSize = Config?.PageSize,
                KeySearch = Config?.KeySearch,
                IsHot = Config?.IsHot,
                IsNew = Config?.IsNew,
            };
            var Data = await GetArticles(model);

            return new AppConfigResponse<object>()
            {
                IsSuccess = Data.IsSuccess,
                Message = Data.Message,
                Data = Data.Data,
                Config = request
            };
        }

        public async Task<AppResponse<object>> GetRelatedArticle(RelatedArticleRequest request)
        {
            var Article = await _context.VwArticleLists.Where(x => x.ArticleId == request.ArticleID && x.SiteId == _siteId && x.Approved == true).Select(x => new
            {
                x.ArticleCatId,
                x.ArticleId
            }).FirstOrDefaultAsync();
            if (Article == null)
            {
                return new AppResponse<object>()
                {
                    IsSuccess = false,
                    Data = null,
                    Message = "Không tìm thấy dữ liệu"
                };
            }
            Expression<Func<VwArticleList, bool>> expression = x =>
                x.Approved == true
                && x.ArticleCatId == Article.ArticleCatId
                && x.SiteId == this._siteId;
            var Data = await _context.VwArticleLists.Where(expression).OrderByDescending(x => x.DateApprove).Take(request.Count + 1).Select(x => new ArticleResponse()
            {
                Title = x.Title,
                DateApprove = x.DateApprove,
                ArticleId = x.ArticleId,
                Author = x.Author,
                Summary = x.Summary,
                IsNew = x.IsNew,
                IsHot = x.IsHot,
                ArticleCatId = x.ArticleCatId,
                ImagePath = x.ImagePath
            }).ToListAsync();
            if (Data == null || Data.Count == 0)
            {
                return new AppResponse<object>
                {
                    Data = Data,
                    IsSuccess = false,
                    Message = "Không có dữ liệu"
                };
            }
            Data = Data.Where(x => x.ArticleId != Article.ArticleId).ToList();
            if (Data.Count == request.Count + 1) Data.RemoveAt(Data.Count - 1);
            foreach (var item in Data)
            {
                item.ImagePath = item.ImagePath.AddFirstHostUrl(_webDomain);
            }
            return new AppResponse<object>
            {
                Data = Data,
                IsSuccess = true,
                Message = "Thành công"
            };
        }

        public async Task<AppResponse<object>> GetHorizontalCategories()
        {
            var categories = await _context.ArticleCategories.Where(x => x.SiteId == _siteId && x.IsHorizontal == true).OrderBy(x => x.Horder).Select(x => new ArticleCategoryResponse()
            {
                Horder = x.Horder,
                ArticleCatId = x.ArticleCatId,
                ParentId = x.ParentId,
                ArticleCatName = x.ArticleCatName,
                ContentType = x.ContentType,
                ShowContent = x.ShowContent,
                Url = x.Url
            }).ToListAsync();
            var config = await _context.AppMobileSettings.FindAsync(_hotizontalCategoriesSettingKey, this._siteId);
            if (config != null && !string.IsNullOrEmpty(config.KeySettingValue))
            {
                var Settings = JsonConvert.DeserializeObject<List<ArticleCategorySetting>>(config.KeySettingValue);
                if (Settings != null && Settings.Count > 0)
                {
                    foreach (var item in categories)
                    {
                        item.Url = item.Url.AddFirstHostUrl(_webDomain);
                        item.Setting = Settings.Find(x => x.ArticleCatId == item.ArticleCatId);
                        if (item.Setting != null && item.Setting.SettingValue != null)
                        {
                            if (item.Setting.SettingValue.ContentType != null) item.ContentType = item.Setting.SettingValue.ContentType.Value;
                            item.Url = item.Setting.SettingValue.Url;
                            if (item.Setting.SettingValue.ArticleCatId != null) item.ArticleCatId = item.Setting.SettingValue.ArticleCatId.Value;
                        }
                    }
                }
            }
            var items = categories.GenerateTree(x => x.ArticleCatId, x => x.ParentId);
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = items
            };
        }

        public async Task<AppResponse<object>> GetSingleArticleByCatId(int CatId)
        {
            var now = DateTime.Now.Date;
            var Article = await _context.VwArticleLists.Where(x =>
                    x.ArticleCatId == CatId
                    && x.Approved == true
                    && (x.StartDate == null || x.StartDate <= now)
                    && (x.EndDate == null || x.EndDate >= now)
                    && (x.DateCreate == null || x.DateCreate <= now)
                    ).OrderByDescending(x => x.DateCreate).Select(x => new
                    {
                        x.ArticleId
                    }).FirstOrDefaultAsync();
            if (Article == null) return new AppResponse<object>()
            {
                IsSuccess = false,
                Message = "Không tìm thấy dữ liệu"
            };
            return await GetArticleDetail(Article.ArticleId);
        }

        public async Task<AppResponse<object>> GetVerticalCategories()
        {
            var categories = await _context.ArticleCategories.Where(x => x.SiteId == _siteId && x.IsVertical == true).OrderBy(x => x.Vorder).Select(x => new ArticleCategoryResponse()
            {
                VOrder = x.Vorder,
                ArticleCatId = x.ArticleCatId,
                ParentId = x.ParentId,
                ArticleCatName = x.ArticleCatName,
                ContentType = x.ContentType,
                ShowContent = x.ShowContent,
                Url = x.Url,
            }).ToListAsync();
            var config = await _context.AppMobileSettings.FindAsync(_verticalCategoriesSettingKey, this._siteId);
            if (config != null && !string.IsNullOrEmpty(config.KeySettingValue))
            {
                var Settings = JsonConvert.DeserializeObject<List<ArticleCategorySetting>>(config.KeySettingValue);
                if (Settings != null && Settings.Count > 0)
                {
                    foreach (var item in categories)
                    {
                        item.Url = item.Url.AddFirstHostUrl(_webDomain);
                        item.Setting = Settings.Find(x => x.ArticleCatId == item.ArticleCatId);
                        if (item.Setting != null && item.Setting.SettingValue != null)
                        {
                            if (item.Setting.SettingValue.ContentType != null) item.ContentType = item.Setting.SettingValue.ContentType.Value;
                            item.Url = item.Setting.SettingValue.Url;
                            if (item.Setting.SettingValue.ArticleCatId != null) item.ArticleCatId = item.Setting.SettingValue.ArticleCatId.Value;
                        }
                    }
                }
            }
            var items = categories.GenerateTree(x => x.ArticleCatId, x => x.ParentId);
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = items
            };
        }

        public async Task<AppResponse<object>> GetSubCategories(int ArticleCatId)
        {
            var categories = await _context.ArticleCategories.Where(x => x.SiteId == _siteId && x.ShowContent == true && x.ParentId == ArticleCatId).OrderBy(x => x.Vorder).Select(x => new ArticleCategoryResponse()
            {
                VOrder = x.Vorder,
                ArticleCatId = x.ArticleCatId,
                ParentId = x.ParentId,
                ArticleCatName = x.ArticleCatName,
                ContentType = x.ContentType,
                ShowContent = x.ShowContent,
                Url = x.Url,
            }).ToListAsync();
            var config = await _context.AppMobileSettings.FindAsync(_categoriesSettingKey, this._siteId);
            if (config != null && !string.IsNullOrEmpty(config.KeySettingValue))
            {
                var Settings = JsonConvert.DeserializeObject<List<ArticleCategorySetting>>(config.KeySettingValue);
                if (Settings != null && Settings.Count > 0)
                {
                    foreach (var item in categories)
                    {
                        item.Url = item.Url.AddFirstHostUrl(_webDomain);
                        item.Setting = Settings.Find(x => x.ArticleCatId == item.ArticleCatId);
                        if (item.Setting != null && item.Setting.SettingValue != null)
                        {
                            if (item.Setting.SettingValue.ContentType != null) item.ContentType = item.Setting.SettingValue.ContentType.Value;
                            item.Url = item.Setting.SettingValue.Url;
                            if (item.Setting.SettingValue.ArticleCatId != null) item.ArticleCatId = item.Setting.SettingValue.ArticleCatId.Value;
                        }
                    }
                }
            }
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = categories
            };
        }

        public async Task<AppResponse<object>> PushArticle(PushArticle request)
        {
            string SQL = @"
                WITH ctetable(SiteId, parent, depth, path) as 
                (
                    SELECT S.SiteId, S.ParentId, 1 AS depth, convert(varchar(100), S.SiteId) AS path
                    FROM Site as S
                    UNION ALL
                    SELECT S2.SiteId, p.parent, p.depth + 1 AS depth, convert(varchar(100), (RTRIM(p.path) +'->'+ convert(varchar(100), S2.SiteId)))
                    FROM ctetable AS p JOIN Site as S2 on S2.ParentId = p.SiteId
                    WHERE p.parent is not null
                )
                select t.TokenId, t.TokenKey, t.AccessToken, t.SiteId, t.Share from SpeechToTextAccount t
                INNER JOIN ctetable on (t.SiteId = ctetable.SiteId) or (t.SiteId = ctetable.parent and t.Share =1)
                WHERE ctetable.SiteId = @SiteId and t.TokenId !='' and t.TokenId is not null";
            var tokens = await _context.SpeechToTextAccounts.FromSqlRaw(SQL, new SqlParameter("SiteId", _siteId)).ToListAsync();
            if(tokens == null || tokens.Count == 0) {
                return new AppResponse<object>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không tìm thấy thông tin cấu hình chuyển đổi giọng nói"
                };
            }
            using (var client = new HttpClient())
            {
                var token = tokens[0];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Token-id", token.TokenId);
                client.DefaultRequestHeaders.Add("Token-key", token.TokenKey);
                string filePath = @"C:\Users\mrshi\OneDrive\Desktop\aa.mp3";

                var bytes = System.IO.File.ReadAllBytes(filePath);
                ByteArrayContent content = new ByteArrayContent(bytes);
                content.Headers.Add("content-type", "application/octet-stream");
                var response = await client.PostAsync("https://api.idg.vnpt.vn/tts-service/v3/standard", content);
                var contentResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("File uploaded successfully.");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }


                return new AppResponse<object>();
            }
            
        }
      
    }
}
