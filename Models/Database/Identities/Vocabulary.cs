using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class Vocabulary
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public string Word { get; set; } = null!;

    public string Meaning { get; set; } = null!;

    public string? Phonetic { get; set; }

    public string? AudioUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? ExampleSentence { get; set; }

    public DateTime? CreatedDate { get; set; }
}
