#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Blog.Models;

[Table(nameof(Post))]
public partial class Post
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(450)]
    [ForeignKey(nameof(Author))]
    public string AuthorId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateAt { get; set; }

    [StringLength(50)]
    [Column(TypeName = "varchar")]
    public string Slug { get; set; }

    [StringLength(256)]
    [Column(TypeName = "nvarchar")]
    public string Title { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Content { get; set; }


    public IdentityUser Author { get; set; }

    public List<ImageLink> ImageLinks { get; set; }
    
    public ICollection<PostClassify> PostClassifies { get; set; }


    public Post()
    {

    }    

    public Post(string title, string content)
    {
        Title = title;
        Content = content;
    }
}