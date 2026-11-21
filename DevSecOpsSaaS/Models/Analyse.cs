using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Analyse
{
    public int AnalyseId { get; set; }

    public DateTime DateAnalyse { get; set; }

    public string Outil { get; set; } = null!;

    public string CommitSha { get; set; } = null!;

    public string Branche { get; set; } = null!;

    public string NumeroBuild { get; set; } = null!;

    public int NbVulnerabilites { get; set; }

    public decimal ScoreMax { get; set; }

    public int ProjetId { get; set; }

    public int ClePipelineId { get; set; }

    public virtual ClePipeline ClePipeline { get; set; } = null!;

    public virtual Projet Projet { get; set; } = null!;

    public virtual ICollection<Vulnerabilite> Vulnerabilites { get; set; } = new List<Vulnerabilite>();
}
