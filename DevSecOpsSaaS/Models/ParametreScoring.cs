using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class ParametreScoring
{
    public int ParametreScoringId { get; set; }

    public int OrganisationId { get; set; }

    public decimal FacteurFaible { get; set; }

    public decimal FacteurMoyenne { get; set; }

    public decimal FacteurHaute { get; set; }

    public decimal FacteurCritique { get; set; }

    public decimal FacteurInternet { get; set; }

    public decimal FacteurInterne { get; set; }

    public decimal SeuilAlerte { get; set; }

    public string? TelegramChatId { get; set; }

    public virtual Organisation Organisation { get; set; } = null!;
}
