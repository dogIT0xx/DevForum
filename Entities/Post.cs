#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Blog.Entities
{
    [Table(nameof(Post))]
    public class Post
    {
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string ThumbnailPath { get; set; }

        [Required]
        [StringLength(450)]
        [ForeignKey(nameof(Author))]
        public string AuthorId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreateAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdateAt { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar")]
        public string Slug { get; set; }

        [Required]
        [StringLength(256)]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }


        public IdentityUser Author { get; set; }

        public List<PostImage> PostImages { get; set; }

        public ICollection<PostClassify> PostClassifies { get; set; }
    }
}
