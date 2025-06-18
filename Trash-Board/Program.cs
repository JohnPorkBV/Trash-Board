using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TrashBoard.Components;
using TrashBoard.Data;
using TrashBoard.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);
var sqlConnectionString = builder.Configuration.GetValue<string>("SqlConnectionStringLocal");
var AiApiEndpoint = builder.Configuration.GetValue<string>("AiApiEndpoint");
builder.Services.AddSingleton(typeof(CustomLocalizer<>));
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] {
        new CultureInfo("nl"),
        new CultureInfo("en"),
        new CultureInfo("de"), };
    options.DefaultRequestCulture = new RequestCulture("nl");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();

// DB context
builder.Services.AddDbContextFactory<TrashboardDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

// Data Services
builder.Services.AddScoped<ITrashDataService, TrashDataService>();

// User Service
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddSession();
//builder.Services.AddScoped<UserSessionService>();

// ASP.NET AUTH
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<TrashboardDbContext>()

.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";      // Redirect here if not logged in
    options.AccessDeniedPath = "/login";  // Or a separate "Access Denied" page
});


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();



// Api Services
builder.Services.AddHttpClient<IHolidayService, HolidayService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<IBredaEventService, BredaEventService>();
builder.Services.AddHttpClient<AiPredictionService>(client =>
{
    client.BaseAddress = new Uri(AiApiEndpoint);
});


// Bootstrap
builder.Services.AddBlazorBootstrap();


var app = builder.Build();

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

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

var culture = new CultureInfo("nl");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;


app.Run();
