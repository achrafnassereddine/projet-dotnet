using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Utilisateur
{
    public int UtilisateurId { get; set; }

    public string Email { get; set; } = null!;

    public string MotDePasseHash { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string? Telephone { get; set; }

    public int RoleId { get; set; }

    public bool EstActif { get; set; }

    public DateTime DateCreation { get; set; }

    public int OrganisationId { get; set; }

    public virtual ICollection<Alerte> Alertes { get; set; } = new List<Alerte>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual Organisation Organisation { get; set; } = null!;

    public virtual RoleUtilisateur Role { get; set; } = null!;

    public virtual ICollection<Projet> Projets { get; set; } = new List<Projet>();
}
