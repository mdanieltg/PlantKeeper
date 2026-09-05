using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Database;

public class PlantKeeperDbContext : IdentityDbContext<Keeper, Role, Guid>
{
    public PlantKeeperDbContext()
    {
    }

    public PlantKeeperDbContext(DbContextOptions<PlantKeeperDbContext> options) : base(options)
    {
    }

    public DbSet<PlantSpecies> PlantSpecies { get; init; }
    public DbSet<Plant> Plants { get; init; }
    public DbSet<PottingMix> PottingMixes { get; init; }
    public DbSet<Climate> Climates { get; init; }
    public DbSet<WateringMethod> WateringMethods { get; init; }
    public DbSet<Fertilizer> Fertilizers { get; init; }
    public DbSet<Treatment> Treatments { get; init; }
    public DbSet<WateringLog> WateringLogs { get; init; }
    public DbSet<RepottingLog> RepottingLogs { get; init; }
    public DbSet<ObservationLog> ObservationLogs { get; init; }
    public DbSet<FertilizationLog> FertilizationLogs { get; init; }
    public DbSet<TreatmentLog> TreatmentLogs { get; init; }
    public DbSet<GrowthLog> GrowthLogs { get; init; }
    public DbSet<BeneficialOrganism> BeneficialOrganisms { get; init; }
    public DbSet<Pest> Pests { get; init; }
    public DbSet<PropagationMethod> PropagationMethods { get; init; }
    public DbSet<PropagationBatch> PropagationBatches { get; init; }
    public DbSet<SpeciesFertilizerRecommendation> SpeciesFertilizerRecommendations { get; init; }
    public DbSet<SpeciesTreatmentRecommendation> SpeciesTreatmentRecommendations { get; init; }
    public DbSet<SpeciesPropagationMethod> SpeciesPropagationMethods { get; init; }
    public DbSet<SpeciesCareProfile> SpeciesCareProfiles { get; init; }
    public DbSet<SpeciesToxicityProfile> SpeciesToxicityProfiles { get; init; }
    public DbSet<SpeciesFloweringProfile> SpeciesFloweringProfiles { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Design-time only: `dotnet ef` builds the context through the parameterless
        // constructor and needs a provider registered to produce the model. It never
        // opens a connection here, so no connection string is required.
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // First, not last. This call is what configures the Identity tables, and the fluent
        // API is last-write-wins - run it after the configuration below and Identity would
        // overwrite anything declared here about Keeper or Role. It was a no-op at the
        // bottom of this method while the base type was DbContext; it stopped being one the
        // moment the base type became IdentityDbContext.
        base.OnModelCreating(modelBuilder);

        IdentitySeedData.Seed(modelBuilder);

        modelBuilder.Entity<PlantSpecies>(builder =>
        {
            // Primary key
            builder.HasKey(species => species.Id);

            // Foreign keys
            builder.HasOne(species => species.Climate)
                .WithMany(climate => climate.SpeciesList)
                .HasForeignKey(species => species.ClimateId)
                .IsRequired();
            builder.HasOne(species => species.PottingMix)
                .WithMany(mix => mix.SpeciesList)
                .HasForeignKey(species => species.PottingMixId)
                .IsRequired();

            builder.Property(species => species.Name)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(species => species.ScientificName)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(species => species.NameInEnglish)
                .HasMaxLength(50);
            builder.Property(species => species.FloweringHabit)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(species => species.FertilizationFrequency)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(species => species.FertilizationNotes)
                .HasMaxLength(255);
            builder.Property(species => species.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<SpeciesCareProfile>(builder =>
        {
            // Primary key - shared with the owning species
            builder.HasKey(profile => profile.SpeciesId);

            // Foreign key
            builder.HasOne(profile => profile.Species)
                .WithOne(species => species.Care)
                .HasForeignKey<SpeciesCareProfile>(profile => profile.SpeciesId)
                .IsRequired();

            builder.Property(profile => profile.LightMin)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(profile => profile.LightMax)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(profile => profile.LightNotes)
                .HasMaxLength(100);
            builder.Property(profile => profile.WateringRequirement)
                .HasMaxLength(150)
                .IsRequired();
            builder.Property(profile => profile.SoilPhMin)
                .HasPrecision(3, 1)
                .IsRequired();
            builder.Property(profile => profile.SoilPhMax)
                .HasPrecision(3, 1)
                .IsRequired();
            builder.Property(profile => profile.SoilPhNotes)
                .HasMaxLength(100);
            builder.Property(profile => profile.WindTolerance)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(profile => profile.WindToleranceNotes)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<SpeciesToxicityProfile>(builder =>
        {
            // Primary key - shared with the owning species
            builder.HasKey(profile => profile.SpeciesId);

            // Foreign key
            builder.HasOne(profile => profile.Species)
                .WithOne(species => species.Toxicity)
                .HasForeignKey<SpeciesToxicityProfile>(profile => profile.SpeciesId)
                .IsRequired();

            builder.Property(profile => profile.ToHumans)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(profile => profile.ToHumansNotes)
                .HasMaxLength(150);
            builder.Property(profile => profile.ToPets)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(profile => profile.ToPetsNotes)
                .HasMaxLength(150);
        });

        modelBuilder.Entity<SpeciesFloweringProfile>(builder =>
        {
            // Primary key - shared with the owning species
            builder.HasKey(profile => profile.SpeciesId);

            // Foreign key - optional: a species that does not flower has no row here
            builder.HasOne(profile => profile.Species)
                .WithOne(species => species.Flowering)
                .HasForeignKey<SpeciesFloweringProfile>(profile => profile.SpeciesId)
                .IsRequired();

            builder.Property(profile => profile.BloomSeason)
                .HasMaxLength(150)
                .IsRequired();
            builder.Property(profile => profile.BloomCareNotes)
                .HasMaxLength(255)
                .IsRequired();
            builder.Property(profile => profile.SeedViability)
                .HasMaxLength(255)
                .IsRequired();
            builder.Property(profile => profile.SeedHarvestTiming)
                .HasMaxLength(150)
                .IsRequired();
        });

        modelBuilder.Entity<Plant>(builder =>
        {
            // Primary key
            builder.HasKey(plant => plant.Id);

            // Foreign key
            builder.HasOne(plant => plant.Species)
                .WithMany(species => species.Plants)
                .HasForeignKey(plant => plant.SpeciesId)
                .IsRequired();

            builder.Property(plant => plant.Alias)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(plant => plant.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<PottingMix>(builder =>
        {
            // Primary key
            builder.HasKey(mix => mix.Id);

            builder.Property(mix => mix.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(mix => mix.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Climate>(builder =>
        {
            // Primary key
            builder.HasKey(climate => climate.Id);

            builder.Property(climate => climate.Name)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(climate => climate.Temperature)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(climate => climate.Precipitation)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(climate => climate.Humidity)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(climate => climate.Sun)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(climate => climate.Wind)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(climate => climate.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Fertilizer>(builder =>
        {
            // Primary key
            builder.HasKey(fertilizer => fertilizer.Id);

            builder.Property(fertilizer => fertilizer.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(fertilizer => fertilizer.Description)
                .HasMaxLength(255);
            builder.Property(fertilizer => fertilizer.NpkRatio)
                .HasMaxLength(15);
            builder.Property(fertilizer => fertilizer.Category)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
        });

        modelBuilder.Entity<WateringMethod>(builder =>
        {
            // Primary key
            builder.HasKey(method => method.Id);

            builder.Property(method => method.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(method => method.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Treatment>(builder =>
        {
            // Primary key
            builder.HasKey(treatment => treatment.Id);

            builder.Property(treatment => treatment.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(treatment => treatment.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<FertilizationLog>(builder =>
        {
            // Primary key
            builder.HasKey(log => log.Id);

            // Foreign keys
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.FertilizationLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();
            builder.HasOne(log => log.FertilizerUsed)
                .WithMany(method => method.Logs)
                .HasForeignKey(log => log.FertilizerId)
                .IsRequired();

            builder.Property(log => log.Dose)
                .HasMaxLength(100);
            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<ObservationLog>(builder =>
        {
            // Primary key
            builder.HasKey(log => log.Id);

            // Foreign key
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.ObservationLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();

            builder.Property(log => log.Notes)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<RepottingLog>(builder =>
        {
            // Primary key
            builder.HasKey(log => log.Id);

            // Foreign key
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.RepottingLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();

            builder.Property(log => log.Dimensions)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(log => log.Volume)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(log => log.Material)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<TreatmentLog>(builder =>
        {
            // Primary key
            builder.HasKey(log => log.Id);

            // Foreign keys
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.TreatmentLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();
            builder.HasOne(log => log.Treatment)
                .WithMany(method => method.Logs)
                .HasForeignKey(log => log.TreatmentId)
                .IsRequired();

            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<WateringLog>(builder =>
        {
            // Primary key
            builder.HasKey(log => log.Id);

            // Foreign key
            builder.HasOne(log => log.WateringMethod)
                .WithMany(method => method.Logs)
                .HasForeignKey(log => log.MethodId)
                .IsRequired();

            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<PropagationMethod>(builder =>
        {
            // Primary key
            builder.HasKey(method => method.Id);

            builder.Property(method => method.Name)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(method => method.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Pest>(builder =>
        {
            // Primary key
            builder.HasKey(pest => pest.Id);

            builder.Property(pest => pest.Name)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(pest => pest.Description)
                .HasMaxLength(255);
            builder.Property(pest => pest.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(pest => pest.Notes)
                .HasMaxLength(300);

            // Many-to-many
            builder.HasMany(pest => pest.Treatments)
                .WithMany(treatment => treatment.Pests)
                .UsingEntity(join => join.ToTable("PestTreatments"));
        });

        modelBuilder.Entity<BeneficialOrganism>(builder =>
        {
            // Primary key
            builder.HasKey(organism => organism.Id);

            builder.Property(organism => organism.Name)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(organism => organism.ScientificName)
                .HasMaxLength(100);
            builder.Property(organism => organism.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(organism => organism.SuggestedPlants)
                .HasMaxLength(255);
            builder.Property(organism => organism.Notes)
                .HasMaxLength(300);

            // Many-to-many
            builder.HasMany(organism => organism.SupportingSpecies)
                .WithMany(species => species.BeneficialOrganisms)
                .UsingEntity(join => join.ToTable("SpeciesBeneficialOrganisms"));
            builder.HasMany(organism => organism.PestsControlled)
                .WithMany(pest => pest.ControlledBy)
                .UsingEntity(join => join.ToTable("BeneficialOrganismPests"));
        });

        modelBuilder.Entity<SpeciesFertilizerRecommendation>(builder =>
        {
            // Primary key
            builder.HasKey(recommendation => recommendation.Id);

            // Foreign key
            builder.HasOne(recommendation => recommendation.Species)
                .WithMany(species => species.FertilizerRecommendations)
                .HasForeignKey(recommendation => recommendation.SpeciesId)
                .IsRequired();

            // One cell per species and fertilizer category
            builder.HasIndex(recommendation => new { recommendation.SpeciesId, recommendation.Category })
                .IsUnique();

            builder.Property(recommendation => recommendation.Category)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(recommendation => recommendation.Suitability)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(recommendation => recommendation.Notes)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<SpeciesTreatmentRecommendation>(builder =>
        {
            // Primary key
            builder.HasKey(recommendation => recommendation.Id);

            // Foreign keys
            builder.HasOne(recommendation => recommendation.Species)
                .WithMany(species => species.TreatmentRecommendations)
                .HasForeignKey(recommendation => recommendation.SpeciesId)
                .IsRequired();
            builder.HasOne(recommendation => recommendation.Treatment)
                .WithMany(treatment => treatment.SpeciesRecommendations)
                .HasForeignKey(recommendation => recommendation.TreatmentId)
                .IsRequired();

            // One cell per species and treatment
            builder.HasIndex(recommendation => new { recommendation.SpeciesId, recommendation.TreatmentId })
                .IsUnique();

            builder.Property(recommendation => recommendation.Safety)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(recommendation => recommendation.Notes)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<SpeciesPropagationMethod>(builder =>
        {
            // Primary key
            builder.HasKey(link => link.Id);

            // Foreign keys
            builder.HasOne(link => link.Species)
                .WithMany(species => species.PropagationMethods)
                .HasForeignKey(link => link.SpeciesId)
                .IsRequired();
            builder.HasOne(link => link.Method)
                .WithMany(method => method.SpeciesLinks)
                .HasForeignKey(link => link.PropagationMethodId)
                .IsRequired();

            // One row per species and propagation method
            builder.HasIndex(link => new { link.SpeciesId, link.PropagationMethodId })
                .IsUnique();

            builder.Property(link => link.RootingHormone)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(link => link.Difficulty)
                .HasConversion<string>()
                .HasMaxLength(30);
            builder.Property(link => link.BestSeason)
                .HasMaxLength(50);
            builder.Property(link => link.Notes)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<PropagationBatch>(builder =>
        {
            // Primary key
            builder.HasKey(batch => batch.Id);

            // Foreign keys
            builder.HasOne(batch => batch.Species)
                .WithMany(species => species.PropagationBatches)
                .HasForeignKey(batch => batch.SpeciesId)
                .IsRequired();
            builder.HasOne(batch => batch.SourcePlant)
                .WithMany(plant => plant.PropagationBatches)
                .HasForeignKey(batch => batch.SourcePlantId);
            builder.HasOne(batch => batch.Method)
                .WithMany(method => method.Batches)
                .HasForeignKey(batch => batch.PropagationMethodId);

            builder.Property(batch => batch.Medium)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(batch => batch.RootingHormone)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(batch => batch.Status)
                .HasMaxLength(255);
            builder.Property(batch => batch.TargetTransplantWindow)
                .HasMaxLength(50);
            builder.Property(batch => batch.Notes)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<GrowthLog>(builder =>
        {
            // Primary key
            builder.HasKey(log => log.Id);

            // Foreign key
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.GrowthLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();

            builder.Property(log => log.HeightCm)
                .HasPrecision(6, 1);
            builder.Property(log => log.HeightToLastNodeCm)
                .HasPrecision(6, 1);
            builder.Property(log => log.Notes)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<Keeper>(builder => builder.Property(keeper => keeper.DisplayName).HasMaxLength(100));
        modelBuilder.Entity<Role>(builder => builder.Property(role => role.Description).HasMaxLength(255));
    }
}
