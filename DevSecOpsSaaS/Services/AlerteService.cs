using DevSecOpsSaaS.Data;
using DevSecOpsSaaS.Models;
using DevSecOpsSaaS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DevSecOpsSaaS.Services;

public class AlerteService
{
    private readonly DevSecOpsContext _dbContext;
    private readonly INotificateur _notificateur;

    public AlerteService(DevSecOpsContext dbContext, INotificateur notificateur)
    {
        _dbContext = dbContext;
        _notificateur = notificateur;
    }

    public async Task<bool> TraiterSeuilAlerteAsync(
        Vulnerabilite vulnerabilite,
        Projet projet,
        ParametreScoring parametres,
        bool depasseSeuil,
        CancellationToken cancellationToken = default)
    {
        if (!depasseSeuil)
        {
            return false;
        }

        int statutNouvelleId = await ObtenirStatutAlerteIdAsync(StatutAlerteCode.Nouvelle, cancellationToken);

        var alerteExistante = await _dbContext.Alertes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.VulnerabiliteId == vulnerabilite.VulnerabiliteId, cancellationToken);

        if (alerteExistante is null)
        {
            alerteExistante = new Alerte
            {
                VulnerabiliteId = vulnerabilite.VulnerabiliteId,
                StatutAlerteId = statutNouvelleId,
                DateCreation = DateTime.UtcNow,
                NotificationEnvoyee = false
            };

            _dbContext.Alertes.Add(alerteExistante);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        bool envoyee = await _notificateur.EnvoyerAlerteAsync(
            alerteExistante,
            vulnerabilite,
            projet,
            parametres.TelegramChatId,
            cancellationToken);

        if (envoyee && !alerteExistante.NotificationEnvoyee)
        {
            alerteExistante.NotificationEnvoyee = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task<int> ObtenirStatutAlerteIdAsync(
        StatutAlerteCode code,
        CancellationToken cancellationToken)
    {
        string codeStr = code.ToString();
        return await _dbContext.StatutAlertes
            .IgnoreQueryFilters()
            .Where(s => s.Code == codeStr)
            .Select(s => s.StatutAlerteId)
            .FirstAsync(cancellationToken);
    }
}