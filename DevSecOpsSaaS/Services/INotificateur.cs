using DevSecOpsSaaS.Models;

namespace DevSecOpsSaaS.Services;

public interface INotificateur
{
    Task<bool> EnvoyerAlerteAsync(
        Alerte alerte,
        Vulnerabilite vulnerabilite,
        Projet projet,
        string? telegramChatId,
        CancellationToken cancellationToken = default);
}