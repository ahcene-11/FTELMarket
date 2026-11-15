using FTELMarket.Data;
using FTELMarket.Services;
using FTELMarket.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/ftelmarket-.log",
         rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Enregistrement des services

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<FtelMarketDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FtelMarket")
    )
);

// Configuration CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


//config de flakyapp
builder.Services
    // permet le build du pattern Options
    .AddOptions<FlakySettings>()
     
    .BindConfiguration(nameof(FlakySettings))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<FlakyService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<FlakySettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseAddress);
});



// Construction de l'application
var app = builder.Build();

// Configuration du pipeline HTTP

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("FrontPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();