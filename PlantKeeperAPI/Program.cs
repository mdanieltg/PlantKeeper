using System.Reflection;
using PlantKeeperAPI.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options => options.ReturnHttpNotAcceptable = true);
builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddCors(options =>
{
    options.AddPolicy("devenv", policyBuilder => policyBuilder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("devenv");
}
else
{
    app.UseHsts();
    app.UseCors();
}

// app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
