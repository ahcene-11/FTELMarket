using FTELMarket.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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