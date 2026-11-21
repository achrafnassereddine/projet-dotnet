using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Invitation
{
    public int InvitationId { get; set; }

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public string Jeton { get; set; } = null!;

    public DateTime DateExpiration { get; set; }

    public bool EstUtilisee { get; set; }

    public int OrganisationId { get; set; }

    public int InviteParId { get; set; }

    public virtual Utilisateur InvitePar { get; set; } = null!;

    public virtual Organisation Organisation { get; set; } = null!;

    public virtual RoleUtilisateur Role { get; set; } = null!;
}
