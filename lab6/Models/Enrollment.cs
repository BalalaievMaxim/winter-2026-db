using System;
using System.Collections.Generic;

namespace lab6.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int ClientId { get; set; }

    public int ClassId { get; set; }

    public DateTime? RegistrationTime { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;
}
