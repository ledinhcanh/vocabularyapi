using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class Topic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
}
