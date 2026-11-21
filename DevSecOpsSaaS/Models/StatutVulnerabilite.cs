using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class StatutVulnerabilite
{
    public int StatutVulnerabiliteId { get; set; }

    public string Code { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Vulnerabilite> Vulnerabilites { get; set; } = new List<Vulnerabilite>();
}
