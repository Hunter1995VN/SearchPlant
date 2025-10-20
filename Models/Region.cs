using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Region
{
    public int Regionid { get; set; }

    public string Regionname { get; set; } = null!;

    public string? Regiondescription { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
