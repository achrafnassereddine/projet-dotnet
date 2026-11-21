using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevSecOpsSaaS.ViewModels.Dev;

public class DevDashboardViewModel
{
    public int NbProjetsAffectes { get; set; }
    public int NbVulnerabilitesOuvertes { get; set; }
    public int NbVulnerabilitesEnCours { get; set; }
    public int NbVulnerabilitesCorrigees { get; set; }
}

public class DevProjetItemViewModel
{
    public int ProjetId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string DepotUrl { get; set; } = string.Empty;
    public string CriticiteLibelle { get; set; } = string.Empty;
    public bool ExposeInternet { get; set; }
    public int NbOuvertes { get; set; }
    public int NbEnCours { get; set; }
    public DateTime? DerniereAnalyseDate { get; set; }
}

public class DevVulnerabiliteItemViewModel
{
    public int VulnerabiliteId { get; set; }
    public int ProjetId { get; set; }
    public string ProjetNom { get; set; } = string.Empty;
    public string CveId { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Package { get; set; } = string.Empty;
    public string? Fichier { get; set; }
    public int? Ligne { get; set; }
    public decimal ScoreBrut { get; set; }
    public decimal ScoreReel { get; set; }
    public string GraviteCode { get; set; } = string.Empty;
    public string GraviteLibelle { get; set; } = string.Empty;
    public int StatutVulnerabiliteId { get; set; }
    public string StatutCode { get; set; } = string.Empty;
    public string StatutLibelle { get; set; } = string.Empty;
    public DateTime DatePremiereDetection { get; set; }
    public string? Recommandation { get; set; }
}

public class DevVulnerabilitesListeViewModel
{
    public int? ProjetIdFiltre { get; set; }
    public int? StatutIdFiltre { get; set; }
    public List<SelectListItem> ProjetsAffectes { get; set; } = new();
    public List<SelectListItem> StatutsDisponibles { get; set; } = new();
    public List<DevVulnerabiliteItemViewModel> Vulnerabilites { get; set; } = new();
}

public class ChangerStatutVulnerabiliteViewModel
{
    [Required]
    public int VulnerabiliteId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int NouveauStatutVulnerabiliteId { get; set; }
}