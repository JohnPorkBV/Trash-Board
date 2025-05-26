using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Trash_Board.Components;
using Trash_Board.Data;
using Trash_Board.Services;

var builder = WebApplication.CreateBuilder(args);
var sqlConnectionString = builder.Configuration.GetValue<string>("SqlConnectionStringLocal");

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();

// DB context
builder.Services.AddDbContextFactory<TrashboardDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddScoped<ITrashDataService, TrashDataService>();

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
