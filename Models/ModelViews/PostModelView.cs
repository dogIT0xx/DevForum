using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models.ModelViews
{
    public class PostModelView
    {
        [Required]
        [StringLength(450)]
        public string AuthorId { get; set; }


    }
}
