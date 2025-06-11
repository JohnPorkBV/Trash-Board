using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrashBoard.Components;
using TrashBoard.Data;
using TrashBoard.Services;

var builder = WebApplication.CreateBuilder(args);
var sqlConnectionString = builder.Configuration.GetValue<string>("SqlConnectionStringLocal");

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();

// DB context
builder.Services.AddDbContextFactory<TrashboardDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

// Data Services
builder.Services.AddScoped<ITrashDataService, TrashDataService>();

// Api Services
builder.Services.AddHttpClient<IHolidayService, HolidayService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

// Bootstrap
builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
