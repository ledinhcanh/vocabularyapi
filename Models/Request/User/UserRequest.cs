using System.ComponentModel.DataAnnotations;

namespace API.Models.Request.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = null!;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        [Required(ErrorMessage = "Quyền (Role) không được để trống")]
        public string Role { get; set; } = "User";
        
        public bool? IsActive { get; set; } = true;
    }

    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "ID Người dùng không hợp lệ")]
        public int UserId { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        [Required(ErrorMessage = "Quyền (Role) không được để trống")]
        public string Role { get; set; } = "User";

        public bool? IsActive { get; set; }
        
        // Nếu muốn đổi mật khẩu thì nhập vào đây, để null sẽ giữ nguyên pass cũ
        public string? NewPassword { get; set; }
    }
}
