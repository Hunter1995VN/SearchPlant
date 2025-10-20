using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Fertilizer
{
    public int Fertilizerid { get; set; }

    public string Fertilizername { get; set; } = null!;

    public string? Composition { get; set; }

    public string? Usagenote { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
