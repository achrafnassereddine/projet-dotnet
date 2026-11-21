using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class RoleUtilisateur
{
    public int RoleId { get; set; }

    public string Code { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
