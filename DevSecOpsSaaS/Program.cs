using System.Text;
using DevSecOpsSaaS.Data;
using DevSecOpsSaaS.Security;
using DevSecOpsSaaS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration DbContext (Database First) + contexte multi-tenant
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La chaine de connexion 'DefaultConnection' est introuvable.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ContexteOrganisation>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddScoped<AlerteService>();
builder.Services.AddScoped<RapportService>();

string telegramBaseUrl = builder.Configuration["Telegram:BaseUrl"] ?? "https://api.telegram.org";
builder.Services.AddHttpClient<INotificateur, TelegramNotifier>(client =>
{
    client.BaseAddress = new Uri(telegramBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddDbContext<DevSecOpsContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
});

// 2. Double authentification : JwtBearer (Cookie HttpOnly) + ApiKey (Pipeline CI/CD)
string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La cle secrete 'Jwt:Key' est introuvable (verifiez dotnet user-secrets).");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DevSecOpsSaaS";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DevSecOpsSaaS.Users";
string jwtCookieName = builder.Configuration["Jwt:CookieName"] ?? "access_token";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token)
                    && context.Request.Cookies.TryGetValue(jwtCookieName, out string? cookieToken)
                    && !string.IsNullOrWhiteSpace(cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Pour les requetes API /api/*, conserver la reponse 401 standard
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    return Task.CompletedTask;
                }

                // Pour les requetes MVC, rediriger vers /Compte/Connexion avec returnUrl encodee
                context.HandleResponse();
                string returnUrl = Uri.EscapeDataString(
                    context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Compte/Connexion?returnUrl={returnUrl}");
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.SchemeName, _ => { });

// 3. Autorisation & Anti-CSRF global sur MVC
builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// 4. Pipeline HTTP : Authentification -> Synchronisation ContexteOrganisation -> Autorisation
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.Use(async (httpContext, next) =>
{
    var ctxOrg = httpContext.RequestServices.GetRequiredService<ContexteOrganisation>();
    var dbContext = httpContext.RequestServices.GetRequiredService<DevSecOpsContext>();

    if (ctxOrg.EstAuthentifieHumain)
    {
        dbContext.OrganisationIdCourante = ctxOrg.OrganisationId;
        dbContext.UtilisateurIdCourant = ctxOrg.UtilisateurId;
        dbContext.FiltrerParAffectationDeveloppeur = ctxOrg.EstDeveloppeur;
    }
    else if (ctxOrg.EstAuthentifiePipeline)
    {
        dbContext.OrganisationIdCourante = ctxOrg.OrganisationId;
        dbContext.UtilisateurIdCourant = null;
        dbContext.FiltrerParAffectationDeveloppeur = false;
    }
    else
    {
        dbContext.OrganisationIdCourante = null;
        dbContext.UtilisateurIdCourant = null;
        dbContext.FiltrerParAffectationDeveloppeur = false;
    }

    await next(httpContext);
});

app.UseAuthorization();

// 5. Routage des 3 Areas (Admin, Dev, Soc) + controleurs racine
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();