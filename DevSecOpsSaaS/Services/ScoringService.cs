using DevSecOpsSaaS.Data;
using DevSecOpsSaaS.Models;
using DevSecOpsSaaS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DevSecOpsSaaS.Services;

public record ResultatScoring(decimal ScoreReel, bool DepasseSeuil);

public class ScoringService
{
    private readonly DevSecOpsContext _dbContext;

    public ScoringService(DevSecOpsContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ParametreScoring> ObtenirOuCreerParametresAsync(
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        var parametres = await _dbContext.ParametreScorings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.OrganisationId == organisationId, cancellationToken);

        if (parametres is not null)
        {
            return parametres;
        }

        parametres = new ParametreScoring
        {
            OrganisationId = organisationId,
            FacteurFaible = 1.00m,
            FacteurMoyenne = 1.50m,
            FacteurHaute = 2.00m,
            FacteurCritique = 2.50m,
            FacteurInternet = 1.50m,
            FacteurInterne = 1.00m,
            SeuilAlerte = 8.00m,
            TelegramChatId = null
        };

        _dbContext.ParametreScorings.Add(parametres);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return parametres;
    }

    public ResultatScoring CalculerScore(
        decimal scoreBrut,
        GraviteCode gravite,
        bool exposeInternet,
        ParametreScoring parametres)
    {
        decimal facteurGravite = gravite switch
        {
            GraviteCode.Faible => parametres.FacteurFaible,
            GraviteCode.Moyenne => parametres.FacteurMoyenne,
            GraviteCode.Haute => parametres.FacteurHaute,
            GraviteCode.Critique => parametres.FacteurCritique,
            _ => parametres.FacteurFaible
        };

        decimal facteurExposition = exposeInternet
            ? parametres.FacteurInternet
            : parametres.FacteurInterne;

        decimal scoreCalcule = Math.Round(
            scoreBrut * facteurGravite * facteurExposition,
            2,
            MidpointRounding.AwayFromZero);

        bool depasseSeuil = scoreCalcule >= parametres.SeuilAlerte;
        return new ResultatScoring(scoreCalcule, depasseSeuil);
    }

    public async Task<int> RecalculerScoresOrganisationAsync(
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        var parametres = await ObtenirOuCreerParametresAsync(organisationId, cancellationToken);

        var gravitesById = await _dbContext.NiveauGravites
            .IgnoreQueryFilters()
            .ToDictionaryAsync(g => g.GraviteId, g => g.Code, cancellationToken);

        var vulnerabilites = await _dbContext.Vulnerabilites
            .IgnoreQueryFilters()
            .Include(v => v.Projet)
            .Where(v => v.Projet.OrganisationId == organisationId)
            .ToListAsync(cancellationToken);

        foreach (var vuln in vulnerabilites)
        {
            string codeGravite = gravitesById.TryGetValue(vuln.GraviteId, out string? code)
                ? code
                : GraviteCode.Faible.ToString();

            Enum.TryParse<GraviteCode>(codeGravite, ignoreCase: true, out GraviteCode graviteEnum);

            var resultat = CalculerScore(
                vuln.ScoreBrut,
                graviteEnum,
                vuln.Projet.ExposeInternet,
                parametres);

            vuln.ScoreReel = resultat.ScoreReel;
        }

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}