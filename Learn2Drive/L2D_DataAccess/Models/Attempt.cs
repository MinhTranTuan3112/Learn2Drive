﻿using System;
using System.Collections.Generic;

namespace L2D_DataAccess.Models;

public partial class Attempt
{
    public Guid AttemptId { get; set; }

    public Guid? UserId { get; set; }

    public int QuizId { get; set; }

    public DateTime? AttemptDate { get; set; }

    public decimal? Grade { get; set; }

    public virtual ICollection<AttemptDetail> AttemptDetails { get; set; } = new List<AttemptDetail>();

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User User { get; set; }
}