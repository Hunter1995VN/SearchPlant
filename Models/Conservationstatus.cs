using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Conservationstatus
{
    public int Statusid { get; set; }

    public string Statusname { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
