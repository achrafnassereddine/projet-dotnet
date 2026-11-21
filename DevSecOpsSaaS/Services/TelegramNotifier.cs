using System.Net.Http.Json;
using DevSecOpsSaaS.Models;

namespace DevSecOpsSaaS.Services;

public class TelegramNotifier : INotificateur
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotifier> _logger;

    public TelegramNotifier(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramNotifier> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> EnvoyerAlerteAsync(
        Alerte alerte,
        Vulnerabilite vulnerabilite,
        Projet projet,
        string? telegramChatId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(telegramChatId))
        {
            _logger.LogInformation(
                "Notification Telegram ignoree pour l'alerte {AlerteId} : aucun TelegramChatId configure.",
                alerte.AlerteId);
            return false;
        }

        string? botToken = _configuration["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(botToken))
        {
            _logger.LogWarning(
                "Notification Telegram ignoree pour l'alerte {AlerteId} : Telegram:BotToken absent.",
                alerte.AlerteId);
            return false;
        }

        string message =
            $"[DevSecOpsSaaS - Alerte SOC #{alerte.AlerteId}]\n" +
            $"Projet : {projet.Nom} (Expose Internet : {(projet.ExposeInternet ? "Oui" : "Non")})\n" +
            $"Faille : {vulnerabilite.CveId} - {vulnerabilite.Titre}\n" +
            $"Package : {vulnerabilite.Package}\n" +
            $"Score reel : {vulnerabilite.ScoreReel:0.00} (Brut : {vulnerabilite.ScoreBrut:0.00})";

        string url = $"/bot{botToken}/sendMessage";

        try
        {
            var payload = new
            {
                chat_id = telegramChatId.Trim(),
                text = message,
                disable_web_page_preview = true
            };

            using var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Echec HTTP ({StatusCode}) lors de l'envoi Telegram pour l'alerte {AlerteId}.",
                    (int)response.StatusCode,
                    alerte.AlerteId);
                return false;
            }

            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Erreur reseau ou delai expire lors de l'envoi Telegram pour l'alerte {AlerteId}.",
                alerte.AlerteId);
            return false;
        }
    }
}