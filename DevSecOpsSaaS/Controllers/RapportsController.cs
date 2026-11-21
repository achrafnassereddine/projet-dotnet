using DevSecOpsSaaS.DTOs;
using DevSecOpsSaaS.Security;
using DevSecOpsSaaS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSecOpsSaaS.Controllers;

[ApiController]
[IgnoreAntiforgeryToken]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.SchemeName)]
public class RapportsController : ControllerBase
{
    private readonly RapportService _rapportService;
    private readonly ContexteOrganisation _contexteOrganisation;
    private readonly ILogger<RapportsController> _logger;

    public RapportsController(
        RapportService rapportService,
        ContexteOrganisation contexteOrganisation,
        ILogger<RapportsController> logger)
    {
        _rapportService = rapportService;
        _contexteOrganisation = contexteOrganisation;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RapportResultatDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostRapport(
        [FromBody] RapportDto? rapport,
        CancellationToken cancellationToken)
    {
        if (!_contexteOrganisation.ProjetIdPipeline.HasValue
            || !_contexteOrganisation.ClePipelineId.HasValue
            || !_contexteOrganisation.OrganisationId.HasValue)
        {
            return Unauthorized(new
            {
                message = "Authentification pipeline invalide : contexte projet ou organisation manquant."
            });
        }

        if (rapport is null || !ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            RapportResultatDto resultat = await _rapportService.IngererRapportAsync(
                _contexteOrganisation.ProjetIdPipeline.Value,
                _contexteOrganisation.ClePipelineId.Value,
                rapport,
                cancellationToken);

            _logger.LogInformation(
                "Rapport ingere : AnalyseId={AnalyseId}, ProjetId={ProjetId}, OrgId={OrgId}, ScoreMax={ScoreMax}, Bloquant={Bloquant}",
                resultat.AnalyseId,
                resultat.ProjetId,
                _contexteOrganisation.OrganisationId.Value,
                resultat.ScoreMax,
                resultat.Bloquant);

            return StatusCode(StatusCodes.Status201Created, resultat);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}