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
builder.Services.AddCorsPolicies();
builder.Services.AddMapping();
builder.Services.AddApiDocumentation();

builder.Services.AddScoped<IPlantSpeciesService, PlantSpeciesService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiDocumentation();

if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseCorsPolicies();

// app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
