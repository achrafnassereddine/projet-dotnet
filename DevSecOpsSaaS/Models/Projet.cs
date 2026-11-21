using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Projet
{
    public int ProjetId { get; set; }

    public string Nom { get; set; } = null!;

    public string DepotUrl { get; set; } = null!;

    public int CriticiteId { get; set; }

    public bool ExposeInternet { get; set; }

    public DateTime DateCreation { get; set; }

    public int OrganisationId { get; set; }

    public virtual ICollection<Analyse> Analyses { get; set; } = new List<Analyse>();

    public virtual ICollection<ClePipeline> ClePipelines { get; set; } = new List<ClePipeline>();

    public virtual NiveauCriticite Criticite { get; set; } = null!;

    public virtual Organisation Organisation { get; set; } = null!;

    public virtual ICollection<Vulnerabilite> Vulnerabilites { get; set; } = new List<Vulnerabilite>();

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
