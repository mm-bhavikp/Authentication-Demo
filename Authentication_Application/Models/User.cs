using System.ComponentModel.DataAnnotations;

namespace Authentication_Application.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? SecretKey { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
    }
}
