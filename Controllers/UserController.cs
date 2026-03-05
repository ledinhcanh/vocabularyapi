using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Services.Interface;
using API.Models.Request.User;

namespace API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào
    public class UserController : AppControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await _userRepo.GetAllUsers();
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _userRepo.GetUserById(id);
            return Ok(res);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var res = await _userRepo.CreateUser(request);
            if (!res.IsSuccess) return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
        {
            var res = await _userRepo.UpdateUser(request);
            if (!res.IsSuccess) return BadRequest(res);
            return Ok(res);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _userRepo.DeleteUser(id);
            if (!res.IsSuccess) return BadRequest(res);
            return Ok(res);
        }
    }
}
