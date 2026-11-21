using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class NiveauGravite
{
    public int GraviteId { get; set; }

    public string Code { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Vulnerabilite> Vulnerabilites { get; set; } = new List<Vulnerabilite>();
}
