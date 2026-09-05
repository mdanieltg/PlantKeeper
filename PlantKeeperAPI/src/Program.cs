using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers(options => options.ReturnHttpNotAcceptable = true)
    .AddJsonOptions(options => options.JsonSerializerOptions.ApplyPlantKeeperDefaults());

// Applied twice on purpose - see ApplyPlantKeeperDefaults. The line above governs what the
// endpoints serialize; this one governs what the OpenAPI document says they serialize.
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ApplyPlantKeeperDefaults());

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddIdentityFoundation();
builder.Services.AddCookieAuthentication(builder.Environment);
builder.Services.AddPermissionAuthorization();
builder.Services.AddCorsPolicies();
builder.Services.AddMapping();
builder.Services.AddApiDocumentation();

builder.Services.AddScoped<IPlantSpeciesService, PlantSpeciesService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseProxyHeaders();

app.UseApiDocumentation();

if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseCorsPolicies();

app.UseAuthenticationPipeline();
app.MapControllers();

await app.SeedFirstKeeperAsync();

app.Run();

/// <summary>
/// Named so <c>WebApplicationFactory&lt;Program&gt;</c> has an entry point to bootstrap.
/// Top-level statements generate this class as internal, which the test host cannot see.
/// </summary>
public partial class Program;
