using DevSecOpsSaaS.Data;
using DevSecOpsSaaS.DTOs;
using DevSecOpsSaaS.Models;
using DevSecOpsSaaS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DevSecOpsSaaS.Services;

public class RapportService
{
    private readonly DevSecOpsContext _dbContext;
    private readonly ScoringService _scoringService;
    private readonly AlerteService _alerteService;

    public RapportService(
        DevSecOpsContext dbContext,
        ScoringService scoringService,
        AlerteService alerteService)
    {
        _dbContext = dbContext;
        _scoringService = scoringService;
        _alerteService = alerteService;
    }

    public async Task<RapportResultatDto> IngererRapportAsync(
        int projetId,
        int clePipelineId,
        RapportDto rapport,
        CancellationToken cancellationToken = default)
    {
        var projet = await _dbContext.Projets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProjetId == projetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Projet {projetId} introuvable.");

        var clePipeline = await _dbContext.ClePipelines
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ClePipelineId == clePipelineId && !c.EstRevoquee, cancellationToken)
            ?? throw new UnauthorizedAccessException("Cle de pipeline invalide ou revoquee.");

        var parametres = await _scoringService.ObtenirOuCreerParametresAsync(
            projet.OrganisationId,
            cancellationToken);

        var gravitesMap = await _dbContext.NiveauGravites
            .IgnoreQueryFilters()
            .ToDictionaryAsync(g => g.Code, g => g.GraviteId, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var statutsVulnMap = await _dbContext.StatutVulnerabilites
            .IgnoreQueryFilters()
            .ToDictionaryAsync(s => s.Code, s => s.StatutVulnerabiliteId, StringComparer.OrdinalIgnoreCase, cancellationToken);

        int statutOuverteId = statutsVulnMap[StatutVulnerabiliteCode.Ouverte.ToString()];
        int statutEnCoursId = statutsVulnMap[StatutVulnerabiliteCode.EnCoursCorrection.ToString()];
        int statutCorrigeeId = statutsVulnMap[StatutVulnerabiliteCode.Corrigee.ToString()];
        int graviteDefautId = gravitesMap[GraviteCode.Faible.ToString()];

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            DateTime maintenant = DateTime.UtcNow;
            clePipeline.DerniereUtilisation = maintenant;

            var analyse = new Analyse
            {
                DateAnalyse = maintenant,
                Outil = rapport.Outil.Trim(),
                CommitSha = rapport.CommitSha.Trim(),
                Branche = rapport.Branche.Trim(),
                NumeroBuild = rapport.NumeroBuild.Trim(),
                NbVulnerabilites = rapport.Vulnerabilites.Count,
                ScoreMax = 0m,
                ProjetId = projet.ProjetId,
                ClePipelineId = clePipeline.ClePipelineId
            };

            _dbContext.Analyses.Add(analyse);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Normalisation des items entrants (deduplication intra-rapport sur (CveId, Package))
            var itemsUniques = rapport.Vulnerabilites
                .GroupBy(v => (CveId: v.CveId.Trim(), Package: v.Package.Trim()),
                         new CleCvePackageComparer())
                .Select(g => g.OrderByDescending(x => x.ScoreBrut).First())
                .ToList();

            var clesPresentes = itemsUniques
                .Select(x => (CveId: x.CveId.Trim(), Package: x.Package.Trim()))
                .ToHashSet(new CleCvePackageComparer());

            var existantesProjet = await _dbContext.Vulnerabilites
                .IgnoreQueryFilters()
                .Where(v => v.ProjetId == projet.ProjetId)
                .ToListAsync(cancellationToken);

            var existantesParCle = existantesProjet
                .ToDictionary(v => (v.CveId, v.Package), new CleCvePackageComparer());

            int nbNouvelles = 0;
            int nbAlertesGenerees = 0;
            decimal scoreMaxAnalyse = 0m;
            bool auMoinsUneBloquante = false;

            var vulnerabilitesAlerte = new List<(Vulnerabilite Vuln, bool DepasseSeuil)>();

            foreach (var item in itemsUniques)
            {
                string cveId = item.CveId.Trim();
                string pkg = item.Package.Trim();

                GraviteCode graviteEnum = Enum.TryParse<GraviteCode>(item.Gravite.Trim(), true, out var gParsed)
                    ? gParsed
                    : GraviteCode.Faible;

                int graviteId = gravitesMap.TryGetValue(graviteEnum.ToString(), out int gId)
                    ? gId
                    : graviteDefautId;

                var scoring = _scoringService.CalculerScore(
                    item.ScoreBrut,
                    graviteEnum,
                    projet.ExposeInternet,
                    parametres);

                if (scoring.ScoreReel > scoreMaxAnalyse)
                {
                    scoreMaxAnalyse = scoring.ScoreReel;
                }

                if (scoring.DepasseSeuil)
                {
                    auMoinsUneBloquante = true;
                }

                if (existantesParCle.TryGetValue((cveId, pkg), out var vulnExistante))
                {
                    vulnExistante.Titre = item.Titre.Trim();
                    vulnExistante.Description = item.Description.Trim();
                    vulnExistante.Fichier = item.Fichier?.Trim();
                    vulnExistante.Ligne = item.Ligne;
                    vulnExistante.Recommandation = item.Recommandation?.Trim();
                    vulnExistante.ScoreBrut = item.ScoreBrut;
                    vulnExistante.ScoreReel = scoring.ScoreReel;
                    vulnExistante.GraviteId = graviteId;
                    vulnExistante.DateCorrection = null;

                    if (vulnExistante.StatutVulnerabiliteId == statutCorrigeeId)
                    {
                        vulnExistante.StatutVulnerabiliteId = statutOuverteId;
                    }

                    vulnerabilitesAlerte.Add((vulnExistante, scoring.DepasseSeuil));
                }
                else
                {
                    var nouvelleVuln = new Vulnerabilite
                    {
                        CveId = cveId,
                        Titre = item.Titre.Trim(),
                        Description = item.Description.Trim(),
                        Fichier = item.Fichier?.Trim(),
                        Ligne = item.Ligne,
                        Package = pkg,
                        Recommandation = item.Recommandation?.Trim(),
                        ScoreBrut = item.ScoreBrut,
                        ScoreReel = scoring.ScoreReel,
                        GraviteId = graviteId,
                        StatutVulnerabiliteId = statutOuverteId,
                        DatePremiereDetection = maintenant,
                        DateCorrection = null,
                        ProjetId = projet.ProjetId,
                        AnalyseDetecteeId = analyse.AnalyseId
                    };

                    _dbContext.Vulnerabilites.Add(nouvelleVuln);
                    existantesParCle[(cveId, pkg)] = nouvelleVuln;
                    nbNouvelles++;
                    vulnerabilitesAlerte.Add((nouvelleVuln, scoring.DepasseSeuil));
                }
            }

            // Cloture automatique des vulnerabilites actives absentes du rapport courant
            int nbCorrigeesAuto = 0;
            foreach (var vuln in existantesProjet)
            {
                bool estActive = vuln.StatutVulnerabiliteId == statutOuverteId
                              || vuln.StatutVulnerabiliteId == statutEnCoursId;

                if (estActive && !clesPresentes.Contains((vuln.CveId, vuln.Package)))
                {
                    vuln.StatutVulnerabiliteId = statutCorrigeeId;
                    vuln.DateCorrection = maintenant;
                    nbCorrigeesAuto++;
                }
            }

            analyse.ScoreMax = scoreMaxAnalyse;
            analyse.NbVulnerabilites = itemsUniques.Count;

            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var (vuln, depasseSeuil) in vulnerabilitesAlerte)
            {
                bool alerteCreeeOuActive = await _alerteService.TraiterSeuilAlerteAsync(
                    vuln,
                    projet,
                    parametres,
                    depasseSeuil,
                    cancellationToken);

                if (alerteCreeeOuActive)
                {
                    nbAlertesGenerees++;
                }
            }

            await transaction.CommitAsync(cancellationToken);

            return new RapportResultatDto
            {
                AnalyseId = analyse.AnalyseId,
                ProjetId = projet.ProjetId,
                ProjetNom = projet.Nom,
                NbVulnerabilitesDetectees = itemsUniques.Count,
                NbNouvellesVulnerabilites = nbNouvelles,
                NbVulnerabilitesCorrigeesAuto = nbCorrigeesAuto,
                NbAlertesGenerees = nbAlertesGenerees,
                ScoreMax = scoreMaxAnalyse,
                Bloquant = auMoinsUneBloquante
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private sealed class CleCvePackageComparer : IEqualityComparer<(string CveId, string Package)>
    {
        public bool Equals((string CveId, string Package) x, (string CveId, string Package) y) =>
            string.Equals(x.CveId, y.CveId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Package, y.Package, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string CveId, string Package) obj) =>
            HashCode.Combine(
                obj.CveId.ToLowerInvariant(),
                obj.Package.ToLowerInvariant());
    }
}  