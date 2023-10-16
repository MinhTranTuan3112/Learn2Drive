using System;
using System.Collections.Generic;

namespace Driving_License.Models;

public partial class Record
{
    public Guid UserId { get; set; }

    public string LicenseId { get; set; }

    public DateTime? TestDate { get; set; }

    public string TestResult { get; set; }

    public string PhysicalCondition { get; set; }

    public virtual License License { get; set; }

    public virtual User User { get; set; }
}
