using Microsoft.EntityFrameworkCore;
using CryptoTracker.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// EF + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// For CoinGecko requests
builder.Services.AddHttpClient("CoinGecko", client =>
{
    client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "CryptoTracker/1.0");
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS to allow requests from frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Middleware
app.UseCors("AllowFrontend");
app.UseSession();
app.UseStaticFiles(); // files from wwwroot
app.MapControllers();

// Creating db on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Main page index.html
app.MapFallbackToFile("index.html");

app.Run();
