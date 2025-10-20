using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SearchPlant.Models;

public partial class SearchPlantContext : DbContext
{
    public SearchPlantContext()
    {
    }

    public SearchPlantContext(DbContextOptions<SearchPlantContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appuser> Appusers { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommentReaction> CommentReactions { get; set; }

    public virtual DbSet<Conservationstatus> Conservationstatuses { get; set; }

    public virtual DbSet<Fertilizer> Fertilizers { get; set; }

    public virtual DbSet<Growthcycletype> Growthcycletypes { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<Plantdisease> Plantdiseases { get; set; }

    public virtual DbSet<Plantproperty> Plantproperties { get; set; }

    public virtual DbSet<Planttype> Planttypes { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Season> Seasons { get; set; }

    public virtual DbSet<Soiltype> Soiltypes { get; set; }

    public virtual DbSet<Treatment> Treatments { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<Wateringmethod> Wateringmethods { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=SearchPlant;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appuser>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("appuser_pkey");

            entity.ToTable("appuser");

            entity.HasIndex(e => e.Email, "appuser_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Avatarurl).HasColumnName("avatarurl");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerificationToken).HasMaxLength(255);
            entity.Property(e => e.LastOtpSentAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.LastVerificationEmailSentAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");

            entity.ToTable("comments");

            entity.HasIndex(e => new { e.Plantid, e.ParentCommentId }, "idx_comments_plantid_parent_comment_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommentText).HasColumnName("comment_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.Plantid).HasColumnName("plantid");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_parent_comment_id_fkey");

            entity.HasOne(d => d.Plant).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Plantid)
                .HasConstraintName("comments_plantid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("comments_userid_fkey");
        });

        modelBuilder.Entity<CommentReaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comment_reactions_pkey");

            entity.ToTable("comment_reactions");

            entity.HasIndex(e => new { e.CommentId, e.Userid }, "comment_reactions_comment_id_userid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ReactionType).HasColumnName("reaction_type");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentReactions)
                .HasForeignKey(d => d.CommentId)
                .HasConstraintName("comment_reactions_comment_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.CommentReactions)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("comment_reactions_userid_fkey");
        });

        modelBuilder.Entity<Conservationstatus>(entity =>
        {
            entity.HasKey(e => e.Statusid).HasName("conservationstatus_pkey");

            entity.ToTable("conservationstatus");

            entity.Property(e => e.Statusid).HasColumnName("statusid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasColumnName("statusname");
        });

        modelBuilder.Entity<Fertilizer>(entity =>
        {
            entity.HasKey(e => e.Fertilizerid).HasName("fertilizer_pkey");

            entity.ToTable("fertilizer");

            entity.Property(e => e.Fertilizerid).HasColumnName("fertilizerid");
            entity.Property(e => e.Composition).HasColumnName("composition");
            entity.Property(e => e.Fertilizername)
                .HasMaxLength(255)
                .HasColumnName("fertilizername");
            entity.Property(e => e.Usagenote).HasColumnName("usagenote");
        });

        modelBuilder.Entity<Growthcycletype>(entity =>
        {
            entity.HasKey(e => e.Cycleid).HasName("growthcycletype_pkey");

            entity.ToTable("growthcycletype");

            entity.Property(e => e.Cycleid).HasColumnName("cycleid");
            entity.Property(e => e.Cyclename)
                .HasMaxLength(100)
                .HasColumnName("cyclename");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Plantid).HasName("plant_pkey");

            entity.ToTable("plant");

            entity.Property(e => e.Plantid).HasColumnName("plantid");
            entity.Property(e => e.AverageRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("average_rating");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Cycleid).HasColumnName("cycleid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Imagepath).HasColumnName("imagepath");
            entity.Property(e => e.Lastupdated).HasColumnName("lastupdated");
            entity.Property(e => e.Lighttype)
                .HasMaxLength(100)
                .HasColumnName("lighttype");
            entity.Property(e => e.Maxhumidity)
                .HasPrecision(5, 2)
                .HasColumnName("maxhumidity");
            entity.Property(e => e.Maxph)
                .HasPrecision(3, 1)
                .HasColumnName("maxph");
            entity.Property(e => e.Maxtemperature)
                .HasPrecision(5, 2)
                .HasColumnName("maxtemperature");
            entity.Property(e => e.Minhumidity)
                .HasPrecision(5, 2)
                .HasColumnName("minhumidity");
            entity.Property(e => e.Minph)
                .HasPrecision(3, 1)
                .HasColumnName("minph");
            entity.Property(e => e.Mintemperature)
                .HasPrecision(5, 2)
                .HasColumnName("mintemperature");
            entity.Property(e => e.Plantname)
                .HasMaxLength(255)
                .HasColumnName("plantname");
            entity.Property(e => e.RatingCount)
                .HasDefaultValue(0)
                .HasColumnName("rating_count");
            entity.Property(e => e.Scientificname)
                .HasMaxLength(255)
                .HasColumnName("scientificname");
            entity.Property(e => e.Tooltiptext).HasColumnName("tooltiptext");

            entity.HasOne(d => d.Cycle).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Cycleid)
                .HasConstraintName("plant_cycleid_fkey");

            entity.HasMany(d => d.Diseases).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantdiseasemap",
                    r => r.HasOne<Plantdisease>().WithMany()
                        .HasForeignKey("Diseaseid")
                        .HasConstraintName("plantdiseasemap_diseaseid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantdiseasemap_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Diseaseid").HasName("plantdiseasemap_pkey");
                        j.ToTable("plantdiseasemap");
                        j.HasIndex(new[] { "Plantid" }, "idx_plantdiseasemap_plantid");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Diseaseid").HasColumnName("diseaseid");
                    });

            entity.HasMany(d => d.Fertilizers).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantfertilizer",
                    r => r.HasOne<Fertilizer>().WithMany()
                        .HasForeignKey("Fertilizerid")
                        .HasConstraintName("plantfertilizer_fertilizerid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantfertilizer_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Fertilizerid").HasName("plantfertilizer_pkey");
                        j.ToTable("plantfertilizer");
                        j.HasIndex(new[] { "Plantid" }, "idx_plantfertilizer_plantid");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Fertilizerid").HasColumnName("fertilizerid");
                    });

            entity.HasMany(d => d.Properties).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantplantproperty",
                    r => r.HasOne<Plantproperty>().WithMany()
                        .HasForeignKey("Propertyid")
                        .HasConstraintName("plantplantproperty_propertyid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantplantproperty_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Propertyid").HasName("plantplantproperty_pkey");
                        j.ToTable("plantplantproperty");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Propertyid").HasColumnName("propertyid");
                    });

            entity.HasMany(d => d.Regions).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantregion",
                    r => r.HasOne<Region>().WithMany()
                        .HasForeignKey("Regionid")
                        .HasConstraintName("plantregion_regionid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantregion_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Regionid").HasName("plantregion_pkey");
                        j.ToTable("plantregion");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Regionid").HasColumnName("regionid");
                    });

            entity.HasMany(d => d.Seasons).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantseason",
                    r => r.HasOne<Season>().WithMany()
                        .HasForeignKey("Seasonid")
                        .HasConstraintName("plantseason_seasonid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantseason_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Seasonid").HasName("plantseason_pkey");
                        j.ToTable("plantseason");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Seasonid").HasColumnName("seasonid");
                    });

            entity.HasMany(d => d.Soils).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantsoil",
                    r => r.HasOne<Soiltype>().WithMany()
                        .HasForeignKey("Soilid")
                        .HasConstraintName("plantsoil_soilid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantsoil_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Soilid").HasName("plantsoil_pkey");
                        j.ToTable("plantsoil");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Soilid").HasColumnName("soilid");
                    });

            entity.HasMany(d => d.Statuses).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantconservation",
                    r => r.HasOne<Conservationstatus>().WithMany()
                        .HasForeignKey("Statusid")
                        .HasConstraintName("plantconservation_statusid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantconservation_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Statusid").HasName("plantconservation_pkey");
                        j.ToTable("plantconservation");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Statusid").HasColumnName("statusid");
                    });

            entity.HasMany(d => d.Types).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantplanttype",
                    r => r.HasOne<Planttype>().WithMany()
                        .HasForeignKey("Typeid")
                        .HasConstraintName("plantplanttype_typeid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantplanttype_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Typeid").HasName("plantplanttype_pkey");
                        j.ToTable("plantplanttype");
                        j.HasIndex(new[] { "Plantid" }, "idx_plantplanttype_plantid");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Typeid").HasColumnName("typeid");
                    });

            entity.HasMany(d => d.Waterings).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "Plantwatering",
                    r => r.HasOne<Wateringmethod>().WithMany()
                        .HasForeignKey("Wateringid")
                        .HasConstraintName("plantwatering_wateringid_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("Plantid")
                        .HasConstraintName("plantwatering_plantid_fkey"),
                    j =>
                    {
                        j.HasKey("Plantid", "Wateringid").HasName("plantwatering_pkey");
                        j.ToTable("plantwatering");
                        j.IndexerProperty<int>("Plantid").HasColumnName("plantid");
                        j.IndexerProperty<int>("Wateringid").HasColumnName("wateringid");
                    });
        });

        modelBuilder.Entity<Plantdisease>(entity =>
        {
            entity.HasKey(e => e.Diseaseid).HasName("plantdisease_pkey");

            entity.ToTable("plantdisease");

            entity.Property(e => e.Diseaseid).HasColumnName("diseaseid");
            entity.Property(e => e.Cause).HasColumnName("cause");
            entity.Property(e => e.Diseasename)
                .HasMaxLength(255)
                .HasColumnName("diseasename");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");

            entity.HasMany(d => d.Treatments).WithMany(p => p.Diseases)
                .UsingEntity<Dictionary<string, object>>(
                    "Diseasetreatmentmap",
                    r => r.HasOne<Treatment>().WithMany()
                        .HasForeignKey("Treatmentid")
                        .HasConstraintName("diseasetreatmentmap_treatmentid_fkey"),
                    l => l.HasOne<Plantdisease>().WithMany()
                        .HasForeignKey("Diseaseid")
                        .HasConstraintName("diseasetreatmentmap_diseaseid_fkey"),
                    j =>
                    {
                        j.HasKey("Diseaseid", "Treatmentid").HasName("diseasetreatmentmap_pkey");
                        j.ToTable("diseasetreatmentmap");
                        j.IndexerProperty<int>("Diseaseid").HasColumnName("diseaseid");
                        j.IndexerProperty<int>("Treatmentid").HasColumnName("treatmentid");
                    });
        });

        modelBuilder.Entity<Plantproperty>(entity =>
        {
            entity.HasKey(e => e.Propertyid).HasName("plantproperty_pkey");

            entity.ToTable("plantproperty");

            entity.Property(e => e.Propertyid).HasColumnName("propertyid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Propertyname)
                .HasMaxLength(100)
                .HasColumnName("propertyname");
        });

        modelBuilder.Entity<Planttype>(entity =>
        {
            entity.HasKey(e => e.Typeid).HasName("planttype_pkey");

            entity.ToTable("planttype");

            entity.Property(e => e.Typeid).HasColumnName("typeid");
            entity.Property(e => e.Typedescription).HasColumnName("typedescription");
            entity.Property(e => e.Typename)
                .HasMaxLength(255)
                .HasColumnName("typename");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ratings_pkey");

            entity.ToTable("ratings");

            entity.HasIndex(e => new { e.Userid, e.Plantid }, "ratings_userid_plantid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Plantid).HasColumnName("plantid");
            entity.Property(e => e.RatingValue).HasColumnName("rating_value");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Plant).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.Plantid)
                .HasConstraintName("ratings_plantid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("ratings_userid_fkey");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Regionid).HasName("region_pkey");

            entity.ToTable("region");

            entity.Property(e => e.Regionid).HasColumnName("regionid");
            entity.Property(e => e.Regiondescription).HasColumnName("regiondescription");
            entity.Property(e => e.Regionname)
                .HasMaxLength(255)
                .HasColumnName("regionname");
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.Seasonid).HasName("season_pkey");

            entity.ToTable("season");

            entity.Property(e => e.Seasonid).HasColumnName("seasonid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Seasonname)
                .HasMaxLength(100)
                .HasColumnName("seasonname");
        });

        modelBuilder.Entity<Soiltype>(entity =>
        {
            entity.HasKey(e => e.Soilid).HasName("soiltype_pkey");

            entity.ToTable("soiltype");

            entity.Property(e => e.Soilid).HasColumnName("soilid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Soilname)
                .HasMaxLength(255)
                .HasColumnName("soilname");
        });

        modelBuilder.Entity<Treatment>(entity =>
        {
            entity.HasKey(e => e.Treatmentid).HasName("treatment_pkey");

            entity.ToTable("treatment");

            entity.Property(e => e.Treatmentid).HasColumnName("treatmentid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Methodtype)
                .HasMaxLength(100)
                .HasColumnName("methodtype");
            entity.Property(e => e.Precautions).HasColumnName("precautions");
            entity.Property(e => e.Treatmentname)
                .HasMaxLength(255)
                .HasColumnName("treatmentname");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Plantid }).HasName("user_favorites_pkey");

            entity.ToTable("user_favorites");

            entity.HasIndex(e => e.Plantid, "idx_user_favorites_plantid");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Plantid).HasColumnName("plantid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Plant).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.Plantid)
                .HasConstraintName("user_favorites_plantid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("user_favorites_userid_fkey");
        });

        modelBuilder.Entity<Wateringmethod>(entity =>
        {
            entity.HasKey(e => e.Wateringid).HasName("wateringmethod_pkey");

            entity.ToTable("wateringmethod");

            entity.Property(e => e.Wateringid).HasColumnName("wateringid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Methodname)
                .HasMaxLength(255)
                .HasColumnName("methodname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
