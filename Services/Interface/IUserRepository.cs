using API.Models.AppConfig;
using API.Models.Request.User;

namespace API.Services.Interface
{
    public interface IUserRepository
    {
        Task<AppResponse<object>> GetAllUsers();
        Task<AppResponse<object>> GetUserById(int id);
        Task<AppResponse<object>> CreateUser(CreateUserRequest request);
        Task<AppResponse<object>> UpdateUser(UpdateUserRequest request);
        Task<AppResponse<object>> DeleteUser(int id);
    }
}
