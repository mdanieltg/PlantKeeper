using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Database;

public class PlantKeeperDbContext : IdentityDbContext<Keeper, Role, Guid>
{
    private readonly ICurrentKeeper _currentKeeper;

    /// <summary>Design-time only. No request exists and no query runs, so nobody owns anything.</summary>
    public PlantKeeperDbContext() => _currentKeeper = CurrentKeeper.None;

    public PlantKeeperDbContext(DbContextOptions<PlantKeeperDbContext> options, ICurrentKeeper currentKeeper)
        : base(options) => _currentKeeper = currentKeeper;

    public DbSet<PlantSpecies> PlantSpecies { get; init; }
    public DbSet<Plant> Plants { get; init; }
    public DbSet<AlmanacChangeProposal> AlmanacChangeProposals { get; init; }
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

    public override int SaveChanges()
    {
        StampOwnership();
        StampVersions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampOwnership();
        StampVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Assigns every newly added <see cref="IKeeperOwned" /> row to the signed-in keeper,
    /// and refuses to let an existing one change hands.
    /// <para>
    /// Query filters govern reads only - nothing about them sets a value on write - so
    /// without this a created row would carry <see cref="Guid.Empty" /> and become
    /// invisible to everyone including its author. Doing it here rather than in each
    /// controller means the eight cannot drift, and a ninth owned entity is covered the day
    /// it is added. Together with <c>IgnoreOwnership()</c> on the write-side mappings, the
    /// client cannot state ownership and the context always does.
    /// </para>
    /// <para>
    /// An anonymous write would stamp <see cref="Guid.Empty" /> and fail on the foreign key
    /// rather than write an orphan. Every write endpoint requires authentication, so that
    /// path is unreachable; it fails closed if that ever stops being true.
    /// </para>
    /// </summary>
    /// <summary>
    /// Bumps <see cref="IAlmanacVersioned.Version" /> on every almanac row being changed.
    /// <para>
    /// Central, so no caller can forget and no caller can forge one. The version is what a
    /// proposal is written against, so a row that changes without bumping would let a stale
    /// proposal apply silently - the exact failure this phase exists to prevent.
    /// </para>
    /// </summary>
    private void StampVersions()
    {
        foreach (EntityEntry<IAlmanacVersioned> entry in ChangeTracker.Entries<IAlmanacVersioned>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Version = 1;
                    break;

                case EntityState.Modified:
                    entry.Entity.Version++;
                    break;
            }
    }

    private void StampOwnership()
    {
        foreach (EntityEntry<IKeeperOwned> entry in ChangeTracker.Entries<IKeeperOwned>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.KeeperId = _currentKeeper.Id;
                    break;

                // Ownership is immutable. No Input model carries it, so this is a second
                // lock rather than the only one.
                case EntityState.Modified:
                    entry.Property(nameof(IKeeperOwned.KeeperId)).IsModified = false;
                    break;
            }
    }

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

        // Delete behaviour is stated on every relationship below, never left to the
        // default, and the rule is where the row sits rather than what it is:
        //
        //   Restrict  crosses out of one keeper's collection into the shared almanac, or
        //             joins two almanac rows. A cascade here reaches keepers the deleter
        //             cannot see and whose data they have no business removing.
        //   Cascade   stays inside a single aggregate - a plant's logs, a species' profiles
        //             and matrix rows, a keeper's whole tree - or removes a pure link row.
        //   SetNull   the two genuinely optional references on PropagationBatch, which
        //             survive losing what they point at.
        //
        // Restrict means the database refuses the delete; controllers translate that into
        // 409 rather than letting it surface as a 500.

        modelBuilder.Entity<PlantSpecies>(builder =>
        {
            // Primary key
            builder.HasKey(species => species.Id);

            // Foreign keys
            builder.HasOne(species => species.Climate)
                .WithMany(climate => climate.SpeciesList)
                .HasForeignKey(species => species.ClimateId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(species => species.PottingMix)
                .WithMany(mix => mix.SpeciesList)
                .HasForeignKey(species => species.PottingMixId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(log => log.FertilizerUsed)
                .WithMany(method => method.Logs)
                .HasForeignKey(log => log.FertilizerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(log => log.Treatment)
                .WithMany(method => method.Logs)
                .HasForeignKey(log => log.TreatmentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Declared rather than left to convention. It cascaded correctly by discovery,
            // but every other log states this relationship, and an unstated one is exactly
            // what "cascading by omission" looks like.
            builder.HasOne(log => log.Plant)
                .WithMany(plant => plant.WateringLogs)
                .HasForeignKey(log => log.PlantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                // Cascade, by EF's default for an implicit join. Correct here: these rows
                // carry no payload, so deleting either end should just unlink, and there is
                // nothing to lose with the link.
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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(recommendation => recommendation.Treatment)
                .WithMany(treatment => treatment.SpeciesRecommendations)
                .HasForeignKey(recommendation => recommendation.TreatmentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(link => link.Method)
                .WithMany(method => method.SpeciesLinks)
                .HasForeignKey(link => link.PropagationMethodId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Both optional, so the row survives losing either. SetNull rather than the
            // default ClientSetNull: that only nulls what EF happens to have loaded, and
            // leaves the database itself refusing the delete.
            builder.HasOne(batch => batch.SourcePlant)
                .WithMany(plant => plant.PropagationBatches)
                .HasForeignKey(batch => batch.SourcePlantId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(batch => batch.Method)
                .WithMany(method => method.Batches)
                .HasForeignKey(batch => batch.PropagationMethodId)
                .OnDelete(DeleteBehavior.SetNull);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(log => log.HeightCm)
                .HasPrecision(6, 1);
            builder.Property(log => log.HeightToLastNodeCm)
                .HasPrecision(6, 1);
            builder.Property(log => log.Notes)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<AlmanacChangeProposal>(builder =>
        {
            builder.HasKey(proposal => proposal.Id);

            // No navigation to Keeper, and Restrict rather than Cascade: the queue is the
            // almanac's history, and deleting the account of someone who once proposed a
            // change must not erase the record of the change.
            builder.HasOne<Keeper>()
                .WithMany()
                .HasForeignKey(proposal => proposal.ProposedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Keeper>()
                .WithMany()
                .HasForeignKey(proposal => proposal.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            // The moderation queue is one query: pending, oldest first.
            builder.HasIndex(proposal => new { proposal.Status, proposal.ProposedAt });

            builder.Property(proposal => proposal.TargetType)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(proposal => proposal.Operation)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(proposal => proposal.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            // jsonb, not text: it is JSON, and Postgres can be asked about it later without
            // a migration. No length cap - a species aggregate is the biggest body the API
            // accepts and there is no useful number to pick.
            builder.Property(proposal => proposal.ProposedState)
                .HasColumnType("jsonb");

            builder.Property(proposal => proposal.ReviewNote)
                .HasMaxLength(500);
        });

        // No query filter on proposals. They are almanac history, shared like the almanac
        // itself; who may read them is an authorization question, not a tenancy one.

        // Tenancy, last: every IKeeperOwned type gets its column, its index, its foreign key
        // and its filter from one place, so the eight cannot drift apart.
        ConfigureKeeperOwnership<Plant>(modelBuilder);
        ConfigureKeeperOwnership<PropagationBatch>(modelBuilder);
        ConfigureKeeperOwnership<WateringLog>(modelBuilder);
        ConfigureKeeperOwnership<FertilizationLog>(modelBuilder);
        ConfigureKeeperOwnership<TreatmentLog>(modelBuilder);
        ConfigureKeeperOwnership<RepottingLog>(modelBuilder);
        ConfigureKeeperOwnership<ObservationLog>(modelBuilder);
        ConfigureKeeperOwnership<GrowthLog>(modelBuilder);

        // Deliberately no filter on Keeper. Sign-in looks a user up before any tenant
        // context exists, so a self-referential filter would break login outright.
        modelBuilder.Entity<Keeper>(builder => builder.Property(keeper => keeper.DisplayName).HasMaxLength(100));
        modelBuilder.Entity<Role>(builder => builder.Property(role => role.Description).HasMaxLength(255));
    }

    /// <summary>
    /// Scopes one entity type to the signed-in keeper.
    /// <para>
    /// The filter reads <see cref="ICurrentKeeper.Id" /> through the field rather than
    /// capturing a value, so EF compiles it to a query parameter re-evaluated per query
    /// rather than baking one keeper into the cached model. An anonymous request yields
    /// <see cref="Guid.Empty" />, which matches no row - the filter fails closed.
    /// </para>
    /// <para>
    /// The foreign key is declared without navigations on either side. A collection on
    /// <see cref="Keeper" /> would pull the whole domain into the Identity graph, and a
    /// reference here would tempt callers to load it.
    /// </para>
    /// </summary>
    private void ConfigureKeeperOwnership<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IKeeperOwned =>
        modelBuilder.Entity<TEntity>(builder =>
        {
            builder.Property(entity => entity.KeeperId).IsRequired();
            builder.HasIndex(entity => entity.KeeperId);

            builder.HasOne<Keeper>()
                .WithMany()
                .HasForeignKey(entity => entity.KeeperId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(entity => entity.KeeperId == _currentKeeper.Id);
        });
}
