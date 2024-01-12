#nullable disable

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Blog.Utils;

namespace Blog.Models.Account
{
    public class AccountInfoModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email không hợp lệ")]
        [RegularExpression(RegexPatterns.Email)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)] // hiển thị phía client k ảnh hưởng đến server
        [RegularExpression(pattern: RegexPatterns.PhoneNumber, ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }
    }
}
