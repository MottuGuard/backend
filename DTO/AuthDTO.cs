namespace backend.DTO
{
    public class AuthDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class RegisterDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
    public class RefreshDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
