using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class UserFavorite
{
    public int Userid { get; set; }

    public int Plantid { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Plant Plant { get; set; } = null!;

    public virtual Appuser User { get; set; } = null!;
}
