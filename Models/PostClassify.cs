﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blog.Models;

[Table((nameof(PostClassify)))]
[PrimaryKey(nameof(PostId), nameof(TagId))]
public class PostClassify
{
    [ForeignKey(nameof(Post))]
    public Guid PostId { get; set; }


    [ForeignKey(nameof(Tag))]
    public int TagId { get; set; }


    public Post Post { get; set; }

    public Tag Tag { get; set; }
}