using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class Post
{
    public int PostId { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? ViewCount { get; set; }

    public int CategoryId { get; set; }

    public int AuthorId { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
