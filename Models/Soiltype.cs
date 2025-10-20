using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Soiltype
{
    public int Soilid { get; set; }

    public string Soilname { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
