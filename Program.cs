using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register EMITDbContext with PostgreSQL
builder.Services.AddDbContext<EMITDbContext>(options =>
    options.UseNpgsql(
        $"Server={Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost"};" +
        $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
        $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "EMIT_EDT_DB"};" +
        $"User Id={Environment.GetEnvironmentVariable("DB_USER") ?? "postgres"};" +
        $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "tsiririmlay"};"
    )
);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Prevent camelCase conversion to match PascalCase used in JS views
    });

// Allow frontend to call the API (CORS)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API routes (attribute-based)
app.MapControllers();

app.Run();
