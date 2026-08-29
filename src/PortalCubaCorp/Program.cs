using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using PortalCubaCorp.Application;
using PortalCubaCorp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Data Access (CON-003: PostgreSQL via EF Core) ---
builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// --- Infrastructure registrations ---
builder.Services.AddScoped<IPersistence, PersistenceGateway>();
builder.Services.AddScoped<IAuditLogger, AuditInterceptor>();
builder.Services.AddSingleton<ILdapGateway>(sp =>
{
    var ldapSection = builder.Configuration.GetSection("Ldap");
    return new LdapGateway(new LdapGatewayOptions
    {
        Host = ldapSection["Host"] ?? "localhost",
        Port = int.Parse(ldapSection["Port"] ?? "389"),
        BindDn = ldapSection["BindDn"] ?? string.Empty,
        BindPassword = ldapSection["BindPassword"] ?? string.Empty,
        SearchBase = ldapSection["SearchBase"] ?? string.Empty
    }, new PortalCubaCorp.NovellLdapConnectionAdapter());
});

// --- Application service registrations ---
builder.Services.AddScoped<IClockingService, ClockingService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IDirectoryService, DirectoryService>();
builder.Services.AddScoped<IWorkerCategoryService, WorkerCategoryService>();

// --- Authentication (CON-004: Keycloak OIDC client) ---
var keycloakSection = builder.Configuration.GetSection("Keycloak");
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = keycloakSection["Authority"];
    options.ClientId = keycloakSection["ClientId"];
    options.ClientSecret = keycloakSection["ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("roles");
});

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();