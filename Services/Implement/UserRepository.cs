using Microsoft.EntityFrameworkCore;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Request.User;
using API.Services.Interface;
using BCrypt.Net;

namespace API.Services.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly ApiDBContext _context;

        public UserRepository(ApiDBContext context)
        {
            _context = context;
        }

        public async Task<AppResponse<object>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .OrderByDescending(u => u.UserId)
                .ToListAsync();

            return new AppResponse<object> { IsSuccess = true, Data = users };
        }

        public async Task<AppResponse<object>> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return new AppResponse<object> { IsSuccess = false, Message = "Tài khoản không tồn tại" };

            return new AppResponse<object> { IsSuccess = true, Data = user };
        }

        public async Task<AppResponse<object>> CreateUser(CreateUserRequest request)
        {
            try
            {
                // Kiểm tra trùng username
                var exist = await _context.Users.AnyAsync(u => u.Username == request.Username);
                if (exist)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Tên đăng nhập đã tồn tại trong hệ thống." };
                }

                var newUser = new User
                {
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    Email = request.Email,
                    Role = request.Role,
                    IsActive = request.IsActive ?? true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return new AppResponse<object> { IsSuccess = true, Message = "Tạo tài khoản thành công", Data = newUser.UserId };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> UpdateUser(UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    return new AppResponse<object> { IsSuccess = false, Message = "Tài khoản không tồn tại" };

                user.FullName = request.FullName;
                user.Email = request.Email;
                user.Role = request.Role;
                user.IsActive = request.IsActive;

                // Nếu nhập mật khẩu mới thì tiến hành Hash và cập nhật
                if (!string.IsNullOrEmpty(request.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return new AppResponse<object> { IsSuccess = true, Message = "Cập nhật tài khoản thành công" };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return new AppResponse<object> { IsSuccess = false, Message = "Tài khoản không tồn tại" };

                // Logic của app: Không xóa trực tiếp (Hard Delete) mà sẽ disable hoặc kiểm tra liên kết 
                // Ở đây do yêu cầu Admin, cho phép xóa Hard Delete (tuỳ vào ràng buộc ForeignKey SQL)
                // Tuy nhiên LearningProgress đang tham chiếu UserId. Tạm thời Disable sẽ an toàn hơn.
                
                // Mặc định: Ta tiến hành xóa luôn.
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                return new AppResponse<object> { IsSuccess = true, Message = "Đã xóa tài khoản" };
            }
            catch (Exception ex)
            {
                // Conflict Foreign Key
                return new AppResponse<object> { IsSuccess = false, Message = "Lỗi khi xóa tài khoản. Dữ liệu tài khoản này có thể đang ràng buộc với kho lưu trữ từ vựng/báo cáo." };
            }
        }
    }
}
