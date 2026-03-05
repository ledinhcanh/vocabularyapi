using API.Models.AppConfig;
using API.Models.Request.PublicBudget;

namespace API.Services.Interface
{
    public interface IPublicBudgetRepository
    {
        Task<AppResponse<object>> GetCategories();
        Task<AppResponse<object>> GetKyBaoCaos();
        Task<AppPagingResponse<object>> GetListPublicBudget(GetPublicBudgetRequest request);
        Task<AppResponse<object>> GetPublicBudgetDetail(int PublicBudgetId);
    }
}
