using System;
using System.Collections.Generic;

namespace lab6.Models;

public partial class Facilityzone
{
    public int ZoneId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Membershipplan> Plans { get; set; } = new List<Membershipplan>();
}
