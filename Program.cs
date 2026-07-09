using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using QuestPDF.Infrastructure;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Register EMITDbContext with PostgreSQL
builder.Services.AddDbContext<EMITDbContext>(options =>
    options.UseNpgsql(
        $"Server={Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost"};" +
        $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
        $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "EMIT_EDT_DB"};" +
        $"User Id={Environment.GetEnvironmentVariable("DB_USER") ?? "postgres"};" +
        $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "kifeko"};"
    )
);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Prevent camelCase conversion to match PascalCase used in JS views
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Allow frontend to call the API (CORS)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

QuestPDF.Settings.License = LicenseType.Community;

// Register application services
builder.Services.AddScoped<IPlanningService, PlanningService>();
builder.Services.AddScoped<IClasseGenerationService, ClasseGenerationService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();


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
