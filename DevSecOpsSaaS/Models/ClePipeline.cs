using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class ClePipeline
{
    public int ClePipelineId { get; set; }

    public string Nom { get; set; } = null!;

    public string HashCle { get; set; } = null!;

    public string Prefixe { get; set; } = null!;

    public bool EstRevoquee { get; set; }

    public DateTime DateCreation { get; set; }

    public DateTime? DerniereUtilisation { get; set; }

    public int ProjetId { get; set; }

    public virtual ICollection<Analyse> Analyses { get; set; } = new List<Analyse>();

    public virtual Projet Projet { get; set; } = null!;
}
