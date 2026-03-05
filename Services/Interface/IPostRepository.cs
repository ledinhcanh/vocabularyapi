using Azure.Core;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Identities;
using API.Models.Request.Post;

namespace API.Services.Interface
{
    public interface IPostRepository
    {
        public Task<AppPagingResponse<object>> GetPosts(GetPostRequest request);
        public Task<AppResponse<object>> GetSinglePostByCatId(int CatId);
        public Task<AppResponse<object>> GetPostDetail(int PostId);
        public Task<AppResponse<object>> CreatePost(CreatePostRequest request);
        public Task<AppResponse<object>> UpdatePost(UpdatePostRequest request);
        public Task<AppResponse<object>> DeletePost(int id);

    }
}
