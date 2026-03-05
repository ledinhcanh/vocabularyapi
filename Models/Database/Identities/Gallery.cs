using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class Gallery
{
    public int GalleryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int GalleryType { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
