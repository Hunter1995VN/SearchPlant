using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Plantproperty
{
    public int Propertyid { get; set; }

    public string Propertyname { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
