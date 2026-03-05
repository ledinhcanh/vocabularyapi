using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class MediaItem
{
    public int MediaId { get; set; }

    public int GalleryId { get; set; }

    public string Url { get; set; } = null!;

    public string? Caption { get; set; }

    public int? SortOrder { get; set; }
}
