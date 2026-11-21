using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Organisation
{
    public int OrganisationId { get; set; }

    public string Nom { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ParametreScoring? ParametreScoring { get; set; }

    public virtual ICollection<Projet> Projets { get; set; } = new List<Projet>();

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
