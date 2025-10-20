using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Treatment
{
    public int Treatmentid { get; set; }

    public string Treatmentname { get; set; } = null!;

    public string? Methodtype { get; set; }

    public string? Description { get; set; }

    public string? Precautions { get; set; }

    public virtual ICollection<Plantdisease> Diseases { get; set; } = new List<Plantdisease>();
}
