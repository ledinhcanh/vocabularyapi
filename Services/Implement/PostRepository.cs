using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Context;
using API.Models.Handle;
using API.Models.Request.Post;
using API.Models.Response.Post;
using API.Services.Interface;
using System.Linq.Expressions;
using API.Models.Database.Identities;

namespace API.Services.Implement
{
    public class PostRepository : IPostRepository
    {
        private readonly ApiDBContext _context;
        private readonly int _userId;
        private IServiceProvider _services;
        public PostRepository(IServiceProvider services, ApiDBContext ApiDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = ApiDBContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out this._userId);
            this._services = services;
        }

        public async Task<AppPagingResponse<object>> GetPosts(GetPostRequest request)
        {
            Expression<Func<Post, bool>> expression = x =>
                x.IsPublished == true;
            if (request.PostId != null)
            {
                expression = DictionaryApiResponse.AndAlso(expression, e => e.PostId == request.PostId);
            }
            if (!string.IsNullOrEmpty(request.KeySearch) || !string.IsNullOrWhiteSpace(request.KeySearch))
            {
                request.KeySearch = request.KeySearch.Trim();
                expression = DictionaryApiResponse.AndAlso(expression, e => e.Title != null && e.Title.Contains(request.KeySearch));
            }
            var Count = await _context.Posts.Where(expression).CountAsync();
            if (Count == 0)
            {
                return new AppPagingResponse<object>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Không có dữ liệu",
                };
            }

            var Data = await _context.Posts.Where(expression).OrderByDescending(x => x.CreatedAt).Skip(request.PageIndex * request.PageSize).Take(request.PageSize).Select(x => new
            {
                x.Title,
                x.CreatedAt,
                x.PostId,
                x.Summary,
                x.CategoryId,
                x.ThumbnailUrl
            }).ToListAsync();

            return new AppPagingResponse<object>
            {
                Data = Data.Select(x => new
                {
                    x.Title,
                    x.CreatedAt,
                    x.PostId,
                    x.Summary,
                    x.CategoryId,
                    ThumbnailUrl = x.ThumbnailUrl.AddFirstHostUrl(TextUltil.Domain)
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

        public async Task<AppResponse<object>> GetPostDetail(int PostId)
        {
            var Post = await (from p in _context.Posts
                              join c in _context.Categories on p.CategoryId equals c.CategoryId
                              where p.PostId == PostId
                                    && p.IsPublished == true
                              select new PostResponse()
                              {
                                  CategoryId = p.CategoryId,
                                  PostId = p.PostId,
                                  Title = p.Title,
                                  Summary = p.Summary,
                                  ThumbnailUrl = p.ThumbnailUrl,
                                  CreatedAt = p.CreatedAt,
                                  Content = p.Content,
                                  ViewCount = p.ViewCount,
                                  CategorySeo = c.Slug
                              }).FirstOrDefaultAsync();

            if (Post == null) return new AppResponse<object>()
            {
                IsSuccess = false,
                Message = "Không tìm thấy dữ liệu"
            };

            Post.Content = Post.Content?.IncludeDomainToDetail(TextUltil.Domain,
                                    new Dictionary<string, string>() {
                                { "img", "src" },
                                { "video", "src" },
                                { "source", "src" },
                                { "a", "href" },
                                    }, false);
            Post.ThumbnailUrl = Post.ThumbnailUrl?.AddFirstHostUrl(TextUltil.Domain);

            Post.ShareUrl = (Post.CategorySeo + "/" + Post.Title.VnTextToRequestText());

            return new AppResponse<object>
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = Post
            };
        }

        public async Task<AppResponse<object>> GetSinglePostByCatId(int CatId)
        {
            var now = DateTime.Now.Date;
            var post = await _context.Posts.Where(x =>
                    x.PostId == CatId
                    && x.IsPublished == true
                    && (x.CreatedAt == null || x.CreatedAt <= now)
                    ).OrderByDescending(x => x.CreatedAt).Select(x => new
                    {
                        x.PostId
                    }).FirstOrDefaultAsync();
            if (post == null) return new AppResponse<object>()
            {
                IsSuccess = false,
                Message = "Không tìm thấy dữ liệu"
            };
            return await GetPostDetail(post.PostId);
        }

        public async Task<AppResponse<object>> CreatePost(CreatePostRequest request)
        {
            try
            {
                var post = new Post
                {
                    Title = request.Title,
                    Slug = request.Title.GenerateSlug(),
                    Summary = request.Summary,
                    Content = request.Content,
                    ThumbnailUrl = request.ThumbnailUrl,
                    CategoryId = request.CategoryId,
                    IsPublished = request.IsPublished,

                    AuthorId = _userId != 0 ? _userId : 1,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PublishedAt = request.IsPublished ? DateTime.Now : null
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Thêm bài viết thành công",
                    Data = post.PostId
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<AppResponse<object>> UpdatePost(UpdatePostRequest request)
        {
            try
            {
                var post = await _context.Posts.FindAsync(request.PostId);
                if (post == null)
                    return new AppResponse<object> { IsSuccess = false, Message = "Bài viết không tồn tại" };

                post.Title = request.Title;
                post.Slug = request.Title.GenerateSlug();
                post.Summary = request.Summary;
                post.Content = request.Content;
                post.ThumbnailUrl = request.ThumbnailUrl;
                post.CategoryId = request.CategoryId;
                post.IsPublished = request.IsPublished;

                post.UpdatedAt = DateTime.Now;

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công",
                    Data = post.PostId
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<AppResponse<object>> DeletePost(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                    return new AppResponse<object> { IsSuccess = false, Message = "Bài viết không tồn tại" };

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Xóa thành công"
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }
    }
}
