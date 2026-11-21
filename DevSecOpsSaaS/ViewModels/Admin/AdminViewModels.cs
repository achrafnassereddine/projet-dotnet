using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevSecOpsSaaS.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int NbUtilisateursActifs { get; set; }
    public int NbInvitationsEnAttente { get; set; }
    public int NbProjets { get; set; }
    public int NbClesActives { get; set; }
    public int NbVulnerabilitesOuvertes { get; set; }
    public int NbAlertesNouvelles { get; set; }
}

public class UtilisateurItemViewModel
{
    public int UtilisateurId { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public int RoleId { get; set; }
    public string RoleLibelle { get; set; } = string.Empty;
    public bool EstActif { get; set; }
    public DateTime DateCreation { get; set; }
}

public class InvitationItemViewModel
{
    public int InvitationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string RoleLibelle { get; set; } = string.Empty;
    public string Jeton { get; set; } = string.Empty;
    public DateTime DateExpiration { get; set; }
    public bool EstUtilisee { get; set; }
}

public class GestionUtilisateursViewModel
{
    public List<UtilisateurItemViewModel> Utilisateurs { get; set; } = new();
    public List<InvitationItemViewModel> Invitations { get; set; } = new();
    public CreerInvitationViewModel NouvelleInvitation { get; set; } = new();
    public List<SelectListItem> RolesDisponibles { get; set; } = new();
}

public class CreerInvitationViewModel
{
    [Required(ErrorMessage = "L'email du collaborateur est obligatoire.")]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Email a inviter")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le role est obligatoire.")]
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez selectionner un role valide.")]
    [Display(Name = "Role attribue")]
    public int RoleId { get; set; }
}

public class ProjetFormViewModel
{
    public int? ProjetId { get; set; }

    [Required(ErrorMessage = "Le nom du projet est obligatoire.")]
    [StringLength(150)]
    [Display(Name = "Nom du projet")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'URL du depot est obligatoire.")]
    [Url(ErrorMessage = "URL de depot invalide.")]
    [StringLength(500)]
    [Display(Name = "URL du depot Git")]
    public string DepotUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "La criticite metier est obligatoire.")]
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez selectionner un niveau de criticite.")]
    [Display(Name = "Criticite metier")]
    public int CriticiteId { get; set; }

    [Display(Name = "Expose sur Internet")]
    public bool ExposeInternet { get; set; }

    [Display(Name = "Developpeurs affectes")]
    public List<int> DeveloppeursSelectionnesIds { get; set; } = new();

    public List<SelectListItem> CriticitesDisponibles { get; set; } = new();
    public List<SelectListItem> DeveloppeursDisponibles { get; set; } = new();
}

public class ProjetListeItemViewModel
{
    public int ProjetId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string DepotUrl { get; set; } = string.Empty;
    public string CriticiteCode { get; set; } = string.Empty;
    public string CriticiteLibelle { get; set; } = string.Empty;
    public bool ExposeInternet { get; set; }
    public int NbDeveloppeurs { get; set; }
    public int NbVulnerabilitesOuvertes { get; set; }
}

public class CreerCleApiViewModel
{
    [Required(ErrorMessage = "Le libelle de la cle est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Nom de la cle (ex: GitHub Actions Main)")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le projet cible est obligatoire.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Projet")]
    public int ProjetId { get; set; }

    public List<SelectListItem> ProjetsDisponibles { get; set; } = new();
}

public class CleApiItemViewModel
{
    public int ClePipelineId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prefixe { get; set; } = string.Empty;
    public string ProjetNom { get; set; } = string.Empty;
    public bool EstRevoquee { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereUtilisation { get; set; }
}

public class ParametreScoringViewModel
{
    [Required]
    [Range(0.1, 10.0)]
    [Display(Name = "Facteur Criticite Faible")]
    public decimal FacteurFaible { get; set; }

    [Required]
    [Range(0.1, 10.0)]
    [Display(Name = "Facteur Criticite Moyenne")]
    public decimal FacteurMoyenne { get; set; }

    [Required]
    [Range(0.1, 10.0)]
    [Display(Name = "Facteur Criticite Haute")]
    public decimal FacteurHaute { get; set; }

    [Required]
    [Range(0.1, 10.0)]
    [Display(Name = "Facteur Criticite Critique")]
    public decimal FacteurCritique { get; set; }

    [Required]
    [Range(0.1, 10.0)]
    [Display(Name = "Facteur Exposition Internet")]
    public decimal FacteurInternet { get; set; }

    [Required]
    [Range(0.1, 10.0)]
    [Display(Name = "Facteur Application Interne")]
    public decimal FacteurInterne { get; set; }

    [Required]
    [Range(0.0, 100.0)]
    [Display(Name = "Seuil d'alerte SOC / Blocage CI")]
    public decimal SeuilAlerte { get; set; }

    [StringLength(100)]
    [Display(Name = "Telegram Chat ID (optionnel)")]
    public string? TelegramChatId { get; set; }
}