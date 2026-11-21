using System.Security.Claims;
using DevSecOpsSaaS.Models.Enums;

namespace DevSecOpsSaaS.Services;

public class ContexteOrganisation
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContexteOrganisation(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UtilisateurId
    {
        get
        {
            string? raw = User?.FindFirstValue("sub")
                          ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out int id) ? id : null;
        }
    }

    public int? OrganisationId
    {
        get
        {
            string? raw = User?.FindFirstValue("orgId");
            return int.TryParse(raw, out int id) ? id : null;
        }
    }

    public int? ProjetIdPipeline
    {
        get
        {
            string? raw = User?.FindFirstValue("projetId");
            return int.TryParse(raw, out int id) ? id : null;
        }
    }

    public int? ClePipelineId
    {
        get
        {
            string? raw = User?.FindFirstValue("clePipelineId");
            return int.TryParse(raw, out int id) ? id : null;
        }
    }

    public RoleCode? Role
    {
        get
        {
            string? code = User?.FindFirstValue("role")
                         ?? User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<RoleCode>(code, ignoreCase: true, out RoleCode parsed) ? parsed : null;
        }
    }

    public bool EstAuthentifieHumain =>
        User?.Identity?.IsAuthenticated == true
        && OrganisationId.HasValue
        && UtilisateurId.HasValue
        && Role.HasValue;

    public bool EstAuthentifiePipeline =>
        User?.Identity?.IsAuthenticated == true
        && OrganisationId.HasValue
        && ProjetIdPipeline.HasValue
        && ClePipelineId.HasValue;

    public bool EstAdmin => EstAuthentifieHumain && Role == RoleCode.Admin;

    public bool EstDeveloppeur => EstAuthentifieHumain && Role == RoleCode.Developpeur;

    public bool EstAnalysteSoc => EstAuthentifieHumain && Role == RoleCode.AnalysteSOC;
}