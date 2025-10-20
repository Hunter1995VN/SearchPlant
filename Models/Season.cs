using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Season
{
    public int Seasonid { get; set; }

    public string Seasonname { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
