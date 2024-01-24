#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models.Post
{
    public class CreatePostModel
    {
        // Chú ý dùng FormFile sẽ k hoạt động
        public IFormFile Thumbnail {  get; set; } 

        [Required]
        [StringLength(256)]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }
    }
}
