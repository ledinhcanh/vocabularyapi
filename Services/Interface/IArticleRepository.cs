using Azure.Core;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Identities;
using API.Models.Request.Article;

namespace API.Services.Interface
{
    public interface IArticleRepository
    {
        public Task<AppPagingResponse<object>> GetArticles(GetArticleRequest request);
        public Task<AppConfigResponse<object>> GetArticlesByConfig(AppMobileConfig request); 
        public Task<AppResponse<object>> GetHorizontalCategories();
        public Task<AppResponse<object>> GetVerticalCategories();
        public Task<AppResponse<object>> GetSubCategories(int ArticleCatId);
        public Task<AppResponse<object>> GetSingleArticleByCatId(int CatId);
        public Task<AppResponse<object>> GetArticleDetail(int ArticleId);
        public Task<AppPagingResponse<object>> GetArticleFeedback(GetArticleFeedbackRequest request);
        public Task<AppResponse<object>> GetRelatedArticle(RelatedArticleRequest request);
        public Task<AppResponse<object>> PushArticle(PushArticle request);
        public Task<PagingSetting?> GetDefaultPagingSetting();
    }
}
