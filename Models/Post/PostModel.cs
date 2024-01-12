using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models.Post
{
    public class PostModel
    {
        [Required]
        [StringLength(256)]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }

        public PostModel(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }
}
