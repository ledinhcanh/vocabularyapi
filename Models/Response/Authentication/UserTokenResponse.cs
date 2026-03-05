namespace API.Models.Response.Authentication
{
    public class UserTokenResponse
    {
        public string AccessToken { set; get; }
        public DateTime? ValidFrom { set; get; }
        public DateTime? ValidTo { set; get; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}
