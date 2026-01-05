using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Database;

public class PlantKeeperDbContext : DbContext
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseMySql("", ServerVersion.AutoDetect(""));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            builder.Property(species => species.NameInSpanish)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(species => species.Comments)
                .HasMaxLength(255);
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

        base.OnModelCreating(modelBuilder);
    }
}
