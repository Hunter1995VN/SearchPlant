using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Growthcycletype
{
    public int Cycleid { get; set; }

    public string Cyclename { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
