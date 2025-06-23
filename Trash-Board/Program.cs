using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TrashBoard.Components;
using TrashBoard.Data;
using TrashBoard.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrashBoard.Components.Layout;


var builder = WebApplication.CreateBuilder(args);
var sqlConnectionString = builder.Configuration.GetValue<string>("SqlConnectionStringLocal");
var AiApiEndpoint = builder.Configuration.GetValue<string>("AiApiEndpoint");
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl");
var apiKey = builder.Configuration.GetValue<string>("X-API-KEY");

// Language Services
builder.Services.AddScoped(typeof(CustomLocalizer<>));
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


// Api Auto Update
builder.Services.AddHostedService<TrashImportBackgroundService>();


// Api Services
builder.Services.AddHttpClient<IHolidayService, HolidayService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<IBredaEventService, BredaEventService>();
builder.Services.AddHttpClient<IAiPredictionService,AiPredictionService>(client =>
{
    client.BaseAddress = new Uri(AiApiEndpoint);
});
builder.Services.AddHttpClient<IApiTrashDataService, ApiTrashDataService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey!);
});


// Bootstrap
builder.Services.AddBlazorBootstrap();


var app = builder.Build();

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login-handler", async (
    HttpContext http,
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    [FromForm] string username,
    [FromForm] string password) =>
{
    var result = await signInManager.PasswordSignInAsync(username, password, false, false);
    if (result.Succeeded)
    {
        return Results.Redirect("/");
    }

    return Results.Redirect("/login?error=1");
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapPost("/logout-handler", async (
    HttpContext http,
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");

})
.AllowAnonymous()
.DisableAntiforgery();

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

await SeedData.EnsureSeededAsync(app.Services);

app.Run();

