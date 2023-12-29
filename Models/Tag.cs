﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blog.Models;

[Table(nameof(Tag))]
public partial class Tag
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = "nvarchar")]
    [StringLength(256)]
    public string Name { get; set; }

    [Column(TypeName = "varchar")]
    [StringLength(256)]
    public string Slug { get; set; }

    [ForeignKey(nameof(ParentTag))]
    public int? ParentId { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; }


    public Tag ParentTag { get; set; }

    public ICollection<PostClassify> PostClassifies { get; set; }
}