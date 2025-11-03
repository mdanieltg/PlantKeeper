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

    public DbSet<Plant> Plants { get; init; }
    public DbSet<RepottingLog> RepottingLogs { get; init; }
    public DbSet<ObservationLog> ObservationLogs { get; init; }
    public DbSet<WateringMethod> WateringMethods { get; init; }
    public DbSet<WateringLog> WateringLogs { get; init; }
    public DbSet<FertilizationMethod> FertilizationMethods { get; init; }
    public DbSet<FertilizationLog> FertilizationLogs { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseMySql("", ServerVersion.AutoDetect(""));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plant>(builder =>
        {
            builder.HasKey(plant => plant.Id);
            builder.Property(plant => plant.Name)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(plant => plant.NameInEnglish)
                .HasMaxLength(50)
                .IsRequired();
            builder.HasOne(plant => plant.SoilType)
                .WithMany(soil => soil.Plants)
                .HasForeignKey(plant => plant.SoilTypeId)
                .IsRequired();
            builder.HasOne(plant => plant.LightingType)
                .WithMany(lighting => lighting.Plants)
                .HasForeignKey(plant => plant.LightingTypeId)
                .IsRequired();
            builder.Property(plant => plant.HumidityConditions)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(plant => plant.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<FertilizationMethod>(builder =>
        {
            builder.HasKey(method => method.Id);
            builder.Property(method => method.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(method => method.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<WateringMethod>(builder =>
        {
            builder.HasKey(method => method.Id);
            builder.Property(method => method.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(method => method.Description)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<FertilizationLog>(builder =>
        {
            builder.HasKey(log => log.Id);
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.FertilizationLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();
            builder.HasOne(log => log.FertilizationMethod)
                .WithMany(method => method.FertilizationLogs)
                .HasForeignKey(log => log.FertilizationMethodId)
                .IsRequired();
            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<ObservationLog>(builder =>
        {
            builder.HasKey(log => log.Id);
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.ObservationLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();
            builder.Property(log => log.Notes)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<RepottingLog>(builder =>
        {
            builder.HasKey(log => log.Id);
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.RepottingLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired();
            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<WateringLog>(builder =>
        {
            builder.HasKey(log => log.Id);
            builder.HasOne(log => log.WateringMethod)
                .WithMany(method => method.WateringLogs)
                .HasForeignKey(log => log.WateringMethodId)
                .IsRequired();
            builder.Property(log => log.Comments)
                .HasMaxLength(255);
        });

        base.OnModelCreating(modelBuilder);
    }
}
