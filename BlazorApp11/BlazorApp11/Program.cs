using BlazorApp11.Client.Pages;
using BlazorApp11.Client.Services;
using BlazorApp11.Components;
using BlazorApp11.Components.Account;
using BlazorApp11.Data;
using BlazorApp11.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// register server-based implementation to integrate with an API
builder.Services.AddScoped<IApiService, ServerApiService>();

builder.Services.AddHttpClient("LocalAPIClient", cfg =>
{
    cfg.BaseAddress = new Uri(
        builder.Configuration["LocalAPIBaseAddress"] ??
            throw new Exception("LocalAPIBaseAddress is missing."));
});
builder.Services.AddHttpClient("RemoteAPIClient", cfg =>
{
    cfg.BaseAddress = new Uri(
       builder.Configuration["RemoteAPIBaseAddress"] ??
           throw new Exception("RemoteAPIBaseAddress is missing."));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Blazor WebAssembly files are served from the wwwroot folder, so we need to serve static files and
// enable antiforgery protection for the local API.
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp11.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// define a local API for testing
app.MapGet("localapi/bands", () =>
{
    return Results.Ok(new[] { new { Id = 1, Name = "Nirvana (from local API)" },
        new { Id = 2, Name = "Queens of the Stone Age (from local API)" },
        new { Id = 3, Name = "Fred Again. (from local API)" },
        new { Id = 4, Name = "Underworld (from local API)" } });
});

app.Run();
