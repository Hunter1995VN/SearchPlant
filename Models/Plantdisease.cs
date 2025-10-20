using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Plantdisease
{
    public int Diseaseid { get; set; }

    public string Diseasename { get; set; } = null!;

    public string? Symptoms { get; set; }

    public string? Cause { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}
