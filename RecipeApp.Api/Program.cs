using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Services;
using RecipeApp.Repositories;
using RecipeApp.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// ---------------------------
// Repository switch based on appsettings.json
// ---------------------------
bool useDatabase = builder.Configuration.GetValue<bool>("Storage:UseDatabase");

if (useDatabase)
{
    // SQL Server (Docker) setup
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // API repository and service
    builder.Services.AddScoped<IRecipeRepository, ApiRecipeRepository>();
    builder.Services.AddScoped<IRecipeService, ApiRecipeService>();
}
else
{
    // JSON repository (local file) needs IWebHostEnvironment
    builder.Services.AddSingleton<IRecipeRepository, JsonRecipeRepository>(sp =>
    {
        var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<JsonRecipeRepository>>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        return new JsonRecipeRepository(logger, env);
    });

    // JSON-style service
    builder.Services.AddScoped<IRecipeService, RecipeService>();
}

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure SQL database is created if using SQL
if (useDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
}

app.Run();