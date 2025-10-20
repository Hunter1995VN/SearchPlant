using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Plant
{
    public int Plantid { get; set; }

    public string Plantname { get; set; } = null!;

    public string? Scientificname { get; set; }

    public string? Description { get; set; }

    public string? Tooltiptext { get; set; }

    public string? Imagepath { get; set; }

    public int? Cycleid { get; set; }

    public DateTime? Lastupdated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? Minph { get; set; }

    public decimal? Maxph { get; set; }

    public decimal? Minhumidity { get; set; }

    public decimal? Maxhumidity { get; set; }

    public decimal? Mintemperature { get; set; }

    public decimal? Maxtemperature { get; set; }

    public string? Lighttype { get; set; }

    public decimal? AverageRating { get; set; }

    public int? RatingCount { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Growthcycletype? Cycle { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    public virtual ICollection<Plantdisease> Diseases { get; set; } = new List<Plantdisease>();

    public virtual ICollection<Fertilizer> Fertilizers { get; set; } = new List<Fertilizer>();

    public virtual ICollection<Plantproperty> Properties { get; set; } = new List<Plantproperty>();

    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();

    public virtual ICollection<Season> Seasons { get; set; } = new List<Season>();

    public virtual ICollection<Soiltype> Soils { get; set; } = new List<Soiltype>();

    public virtual ICollection<Conservationstatus> Statuses { get; set; } = new List<Conservationstatus>();

    public virtual ICollection<Planttype> Types { get; set; } = new List<Planttype>();

    public virtual ICollection<Wateringmethod> Waterings { get; set; } = new List<Wateringmethod>();
}
