using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using DevSecOpsSaaS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DevSecOpsSaaS.Security;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKey";
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly DevSecOpsContext _dbContext;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        DevSecOpsContext dbContext)
        : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.Fail("En-tete Authorization manquant.");
        }

        string? headerValue = authHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(headerValue)
            || !headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Format Authorization attendu : Bearer <cle_api>.");
        }

        string rawApiKey = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(rawApiKey))
        {
            return AuthenticateResult.Fail("Cle API vide.");
        }

        string hashCle = CalculerSha256Hex(rawApiKey);

        // IgnoreQueryFilters car aucun cookie JWT humain n'est present lors d'un appel pipeline CI/CD
        var cleInfo = await _dbContext.ClePipelines
            .IgnoreQueryFilters()
            .Where(c => c.HashCle == hashCle && !c.EstRevoquee)
            .Select(c => new
            {
                c.ClePipelineId,
                c.ProjetId,
                c.Projet.OrganisationId,
                c.Projet.Nom
            })
            .FirstOrDefaultAsync(Context.RequestAborted);

        if (cleInfo is null)
        {
            return AuthenticateResult.Fail("Cle API invalide ou revoquee.");
        }

        var claims = new List<Claim>
        {
            new("clePipelineId", cleInfo.ClePipelineId.ToString()),
            new("projetId", cleInfo.ProjetId.ToString()),
            new("orgId", cleInfo.OrganisationId.ToString()),
            new("authType", ApiKeyAuthenticationOptions.SchemeName)
        };

        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    public static string CalculerSha256Hex(string valeur)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valeur));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}