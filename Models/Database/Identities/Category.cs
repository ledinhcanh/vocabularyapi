using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? ParentId { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsVisible { get; set; }

    public int Level { get; set; }

    public string? TreePath { get; set; }
}
