using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class NiveauCriticite
{
    public int CriticiteId { get; set; }

    public string Code { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Projet> Projets { get; set; } = new List<Projet>();
}
