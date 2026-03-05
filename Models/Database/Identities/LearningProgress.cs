using System;
using System.Collections.Generic;

namespace API.Models.Database.Identities;

public partial class LearningProgress
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public int VocabId { get; set; }

    public int? Box { get; set; }

    public double? EaseFactor { get; set; }

    public int? Repetitions { get; set; }

    public int? IntervalDays { get; set; }

    public DateTime? NextReviewDate { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public bool? IsMastered { get; set; }
}
