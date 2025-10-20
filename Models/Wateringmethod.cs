using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Wateringmethod
{
    public int Wateringid { get; set; }

    public string Methodname { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
