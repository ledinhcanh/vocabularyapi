using API.Models.AppConfig; 

namespace API.Services.Interface
{
    public interface IBannerFooterRepository
    {
        public Task<AppResponse<object>> GetDefaultImagesBanner();
        public Task<AppResponse<object>> GetBanner();
        public Task<AppResponse<object>> GetFooter();
    }
}
