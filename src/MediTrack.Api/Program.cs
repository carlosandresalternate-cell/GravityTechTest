using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MediTrack.Api.Data;
using MediTrack.Api.Services;
using MediTrack.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// ──── Services ────

// Database (in-memory SQLite for easy setup)
// Keep a persistent connection so the in-memory DB survives across scoped DbContexts
var sqliteConnection = new SqliteConnection("DataSource=:memory:");
sqliteConnection.Open();

builder.Services.AddDbContext<MediTrackDbContext>(options =>
    options.UseSqlite(sqliteConnection));

// Application services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// FluentValidation — register validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientValidator>();

// Caching (Task 5: candidates will use this)
builder.Services.AddMemoryCache();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MediTrack API", Version = "v1" });
});

var app = builder.Build();

// ──── Ensure DB is created with seed data ────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediTrackDbContext>();
    db.Database.EnsureCreated();
}

// ──── Middleware Pipeline ────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Make the implicit Program class public so test projects can reference it
public partial class Program { }
