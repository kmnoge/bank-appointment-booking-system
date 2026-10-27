using System.Text.Json.Serialization;
using BankAppointments.Api.Data;
using BankAppointments.Api.Middleware;
using BankAppointments.Api.Services;
using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using BankAppointments.Api.CQRS.Behaviors;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>));
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<BookingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Bookings")));
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddScoped<IAppointmentCommandService, AppointmentCommandService>();
builder.Services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>()).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHealthChecks().AddDbContextCheck<BookingDbContext>();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapHealthChecks("/health");
app.MapControllers();

// Migrations and reference-data seeding run automatically at startup.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var attempts = 0;
    while (true)
    {
        try { await db.Database.MigrateAsync(); await DbSeeder.SeedAsync(db); break; }
        catch (Exception ex) when (++attempts < 10)
        {
            logger.LogWarning(ex, "Database initialization attempt {Attempt} failed; retrying.", attempts);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
app.Run();

public partial class Program { }
