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

    public DbSet<Plant> Plants { get; set; }
    public DbSet<WateringMethod> WateringMethods { get; set; }
    public DbSet<WateringLog> WateringLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseMySql("", new MySqlServerVersion("8.0.36"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plant>(builder =>
        {
            builder.HasKey(plant => plant.Id);
            builder
                .Property(plant => plant.Name)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(plant => plant.Care)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<WateringMethod>(builder =>
        {
            builder.HasKey(waterMethod => waterMethod.Id);
            builder.Property(method => method.Name)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(method => method.Description)
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
