using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Planttype
{
    public int Typeid { get; set; }

    public string Typename { get; set; } = null!;

    public string? Typedescription { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
