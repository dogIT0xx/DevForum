#nullable disable

using System.ComponentModel.DataAnnotations;

namespace Blog.Models.Account
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
