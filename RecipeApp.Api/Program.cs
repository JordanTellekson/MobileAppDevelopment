using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Services;
using RecipeApp.Repositories;
using RecipeApp.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// SQL Server (Docker) setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// API repositories
builder.Services.AddScoped<IRecipeRepository, ApiRecipeRepository>();
builder.Services.AddScoped<ICategoryRepository, ApiCategoryRepository>();

// Service layer
builder.Services.AddScoped<IRecipeService, ApiRecipeService>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();