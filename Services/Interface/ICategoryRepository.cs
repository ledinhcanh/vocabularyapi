using Azure.Core;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Identities;
using API.Models.Request.Category;

namespace API.Services.Interface
{
    public interface ICategoryRepository
    {
        public Task<AppResponse<object>> GetCategories();
        public Task<AppResponse<object>> GetCategoryById(int id);
        public Task<AppResponse<object>> CreateCategory(CreateCategoryRequest request);
        public Task<AppResponse<object>> UpdateCategory(UpdateCategoryRequest request);
        public Task<AppResponse<object>> DeleteCategory(int id);
    }
}
