using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class StatutAlerte
{
    public int StatutAlerteId { get; set; }

    public string Code { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Alerte> Alertes { get; set; } = new List<Alerte>();
}
