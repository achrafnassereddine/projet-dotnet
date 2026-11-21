using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Vulnerabilite
{
    public int VulnerabiliteId { get; set; }

    public string CveId { get; set; } = null!;

    public string Titre { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Fichier { get; set; }

    public int? Ligne { get; set; }

    public string Package { get; set; } = null!;

    public string? Recommandation { get; set; }

    public decimal ScoreBrut { get; set; }

    public decimal ScoreReel { get; set; }

    public int GraviteId { get; set; }

    public int StatutVulnerabiliteId { get; set; }

    public DateTime DatePremiereDetection { get; set; }

    public DateTime? DateCorrection { get; set; }

    public int ProjetId { get; set; }

    public int AnalyseDetecteeId { get; set; }

    public virtual Alerte? Alerte { get; set; }

    public virtual Analyse AnalyseDetectee { get; set; } = null!;

    public virtual NiveauGravite Gravite { get; set; } = null!;

    public virtual Projet Projet { get; set; } = null!;

    public virtual StatutVulnerabilite StatutVulnerabilite { get; set; } = null!;
}
