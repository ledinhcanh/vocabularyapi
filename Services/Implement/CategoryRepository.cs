using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Context;
using API.Models.Handle;
using API.Models.Request.Category;
using API.Models.Response.Category;
using API.Services.Interface;
using System.Linq.Expressions;
using API.Models.Database.Identities;

namespace API.Services.Implement
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApiDBContext _context;
        private readonly int _userId;
        private IServiceProvider _services;
        public CategoryRepository(IServiceProvider services, ApiDBContext ApiDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = ApiDBContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out this._userId);
            this._services = services;
        }

        public async Task<AppResponse<object>> GetCategories()
        {
            var categories = await _context.Categories.OrderBy(x => x.SortOrder).Select(x => new TopicResponse()
            {
                CategoryId = x.CategoryId,
                Name = x.Name,
                ParentId = x.ParentId,
                Slug = x.Slug,
                IsVisible = x.IsVisible,
            }).ToListAsync();
            var items = categories.GenerateTree(x => x.CategoryId, x => x.ParentId);
            return new AppResponse<object>()
            {
                IsSuccess = true,
                Message = "Thành công",
                Data = items
            };

        }
        public async Task<AppResponse<object>> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return new AppResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục"
                    };
                }

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Thành công",
                    Data = category
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object>
                {
                    IsSuccess = false,
                    Message = "Lỗi: " + ex.Message
                };
            }
        }

        public async Task<AppResponse<object>> CreateCategory(CreateCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Tên danh mục không được để trống" };
                }

                var category = new Category
                {
                    Name = request.Name,
                    ParentId = request.ParentId == 0 ? null : request.ParentId,
                    SortOrder = request.SortOrder,
                    IsVisible = request.IsVisible,
                    Slug = request.Name.GenerateSlug(),
                };

                if (category.ParentId != null)
                {
                    var parent = await _context.Categories.FindAsync(category.ParentId);
                    if (parent != null)
                    {
                        category.Level = parent.Level + 1;
                        category.TreePath = parent.TreePath + "-";
                    }
                }
                else
                {
                    category.Level = 0;
                    category.TreePath = "-";
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                category.TreePath = (category.ParentId != null)
                    ? (_context.Categories.Find(category.ParentId).TreePath + category.CategoryId + "-")
                    : ("-" + category.CategoryId + "-");

                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Thêm danh mục thành công",
                    Data = category
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi hệ thống: " + ex.Message };
            }
        }

        public async Task<AppResponse<object>> UpdateCategory(UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Không tìm thấy danh mục" };
                }

                if (request.ParentId == request.CategoryId)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Danh mục cha không hợp lệ" };
                }

                category.Name = request.Name;
                category.SortOrder = request.SortOrder;
                category.IsVisible = request.IsVisible;
                category.ParentId = request.ParentId == 0 ? null : request.ParentId;

                category.Slug = request.Name.GenerateSlug();

                if (category.ParentId != null)
                {
                    var parent = await _context.Categories.FindAsync(category.ParentId);
                    if (parent != null)
                    {
                        category.Level = parent.Level + 1;
                        category.TreePath = parent.TreePath + category.CategoryId + "-";
                    }
                }
                else
                {
                    category.Level = 0;
                    category.TreePath = "-" + category.CategoryId + "-";
                }

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công",
                    Data = category
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<AppResponse<object>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Danh mục không tồn tại" };
                }

                bool hasChildren = await _context.Categories.AnyAsync(x => x.ParentId == id);
                if (hasChildren)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Phải xóa các danh mục con trước khi xóa danh mục này" };
                }

                _context.Categories.Remove(category);
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
