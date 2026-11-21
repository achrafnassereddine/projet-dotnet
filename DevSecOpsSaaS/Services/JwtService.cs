using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevSecOpsSaaS.Models;
using Microsoft.IdentityModel.Tokens;

namespace DevSecOpsSaaS.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenererJeton(Utilisateur utilisateur, string codeRole)
    {
        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("La cle secrete 'Jwt:Key' est manquante dans la configuration.");
        string issuer = _configuration["Jwt:Issuer"] ?? "DevSecOpsSaaS";
        string audience = _configuration["Jwt:Audience"] ?? "DevSecOpsSaaS.Users";

        int expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out int mins) && mins > 0
            ? mins
            : 480;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, utilisateur.UtilisateurId.ToString()),
            new(ClaimTypes.NameIdentifier, utilisateur.UtilisateurId.ToString()),
            new(JwtRegisteredClaimNames.Email, utilisateur.Email),
            new("role", codeRole),
            new(ClaimTypes.Role, codeRole),
            new("orgId", utilisateur.OrganisationId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string NomCookie => _configuration["Jwt:CookieName"] ?? "access_token";

    public int DureeMinutes => int.TryParse(_configuration["Jwt:ExpirationMinutes"], out int mins) && mins > 0
        ? mins
        : 480;
}