﻿using System;
using System.Collections.Generic;

namespace L2D_DataAccess.Models;

public partial class Teacher
{
    public Guid TeacherId { get; set; }

    public Guid? AccountId { get; set; }

    public string LicenseId { get; set; }

    public string Avatar { get; set; }

    public string FullName { get; set; }

    public string Information { get; set; }

    public string ContactNumber { get; set; }

    public string Email { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<Hire> Hires { get; set; } = new List<Hire>();

    public virtual License License { get; set; }
}
