using System.ComponentModel.DataAnnotations;

namespace API.Models.Request.Authentication
{
    public class UserTokenRequest
    {
        [Required]
        public string Username { set; get; }
        [Required]
        public string Password { set; get; }
    }
}
