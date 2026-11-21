using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevSecOpsSaaS.ViewModels.Soc;

public class SocDashboardViewModel
{
    public int NbAlertesNouvelles { get; set; }
    public int NbAlertesEnCours { get; set; }
    public int NbAlertesResolues { get; set; }
    public int NbFauxPositifs { get; set; }
    public decimal ScoreMaxActif { get; set; }
}

public class SocAlerteItemViewModel
{
    public int AlerteId { get; set; }
    public int StatutAlerteId { get; set; }
    public string StatutCode { get; set; } = string.Empty;
    public string StatutLibelle { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime? DatePriseEnCharge { get; set; }
    public DateTime? DateCloture { get; set; }
    public string? Commentaire { get; set; }
    public bool NotificationEnvoyee { get; set; }
    public string? AnalysteAssigneNom { get; set; }

    public int VulnerabiliteId { get; set; }
    public string CveId { get; set; } = string.Empty;
    public string TitreVulnerabilite { get; set; } = string.Empty;
    public string Package { get; set; } = string.Empty;
    public decimal ScoreBrut { get; set; }
    public decimal ScoreReel { get; set; }
    public string GraviteCode { get; set; } = string.Empty;
    public string ProjetNom { get; set; } = string.Empty;
    public bool ProjetExposeInternet { get; set; }
}

public class SocAlertesListeViewModel
{
    public int? StatutAlerteIdFiltre { get; set; }
    public List<SelectListItem> StatutsDisponibles { get; set; } = new();
    public List<SocAlerteItemViewModel> Alertes { get; set; } = new();
}

public class TraiterAlerteViewModel
{
    [Required]
    public int AlerteId { get; set; }

    [Required(ErrorMessage = "Le nouveau statut de l'alerte est obligatoire.")]
    [Range(1, int.MaxValue)]
    public int NouveauStatutAlerteId { get; set; }

    [StringLength(2000, ErrorMessage = "Le commentaire ne peut pas depasser 2000 caracteres.")]
    [Display(Name = "Commentaire d'investigation / justification")]
    public string? Commentaire { get; set; }
}