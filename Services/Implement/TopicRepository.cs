using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Request.Topic;
using API.Models.Response.Topic;
using API.Services.Interface;
using API.Models.Database.Identities;

namespace API.Services.Implement
{
    public class TopicRepository : ITopicRepository
    {
        private readonly ApiDBContext _context;
        private readonly int _userId;

        public TopicRepository(ApiDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out _userId);
        }

        // 1. Lấy danh sách Topic
        public async Task<AppResponse<object>> GetTopics()
        {
            try
            {
                var topics = await _context.Topics
                    .OrderByDescending(x => x.Id)
                    .Select(x => new TopicResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        ImageUrl = x.ImageUrl
                    }).ToListAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Thành công",
                    Data = topics
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }

        // 2. Lấy chi tiết 1 Topic
        public async Task<AppResponse<object>> GetTopicById(int id)
        {
            try
            {
                var topic = await _context.Topics.FindAsync(id);

                if (topic == null)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Không tìm thấy chủ đề" };
                }

                var responseData = new TopicResponse
                {
                    Id = topic.Id,
                    Name = topic.Name,
                    Description = topic.Description,
                    ImageUrl = topic.ImageUrl
                };

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Thành công",
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }

        // 3. Tạo Topic mới
        public async Task<AppResponse<object>> CreateTopic(CreateTopicRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Tên chủ đề không được để trống" };
                }

                var topic = new Topic
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl
                };

                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Thêm chủ đề thành công",
                    Data = topic
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi hệ thống: " + ex.Message };
            }
        }

        // 4. Cập nhật Topic
        public async Task<AppResponse<object>> UpdateTopic(UpdateTopicRequest request)
        {
            try
            {
                var topic = await _context.Topics.FindAsync(request.Id);
                if (topic == null)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Không tìm thấy chủ đề" };
                }
                topic.Name = request.Name;
                topic.Description = request.Description;
                topic.ImageUrl = request.ImageUrl;

                _context.Topics.Update(topic);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = "Cập nhật thành công",
                    Data = topic
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi: " + ex.Message };
            }
        }

        // 5. Xóa Topic
        public async Task<AppResponse<object>> DeleteTopic(int id)
        {
            try
            {
                var topic = await _context.Topics.FindAsync(id);
                if (topic == null)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Chủ đề không tồn tại" };
                }

                _context.Topics.Remove(topic);
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