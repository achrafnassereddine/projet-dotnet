using System.ComponentModel.DataAnnotations;

namespace DevSecOpsSaaS.DTOs;

public class RapportDto
{
    [Required(ErrorMessage = "Le nom de l'outil d'analyse est obligatoire.")]
    [StringLength(100)]
    public string Outil { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le hash du commit est obligatoire.")]
    [StringLength(100)]
    public string CommitSha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom de la branche est obligatoire.")]
    [StringLength(150)]
    public string Branche { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numero de build est obligatoire.")]
    [StringLength(100)]
    public string NumeroBuild { get; set; } = string.Empty;

    [Required(ErrorMessage = "La liste des vulnerabilites est obligatoire (peut etre vide).")]
    public List<VulnerabiliteItemDto> Vulnerabilites { get; set; } = new();
}

public class VulnerabiliteItemDto
{
    [Required(ErrorMessage = "L'identifiant CVE/CWE est obligatoire.")]
    [StringLength(100)]
    public string CveId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(250)]
    public string Titre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Fichier { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Le numero de ligne doit etre positif.")]
    public int? Ligne { get; set; }

    [Required(ErrorMessage = "Le nom du package est obligatoire.")]
    [StringLength(250)]
    public string Package { get; set; } = string.Empty;

    public string? Recommandation { get; set; }

    [Range(0.0, 10.0, ErrorMessage = "Le score brut CVSS doit etre compris entre 0 et 10.")]
    public decimal ScoreBrut { get; set; }

    [Required(ErrorMessage = "Le code de gravite est obligatoire (Faible, Moyenne, Haute, Critique).")]
    [StringLength(50)]
    public string Gravite { get; set; } = string.Empty;
}

public class RapportResultatDto
{
    public int AnalyseId { get; set; }
    public int ProjetId { get; set; }
    public string ProjetNom { get; set; } = string.Empty;
    public int NbVulnerabilitesDetectees { get; set; }
    public int NbNouvellesVulnerabilites { get; set; }
    public int NbVulnerabilitesCorrigeesAuto { get; set; }
    public int NbAlertesGenerees { get; set; }
    public decimal ScoreMax { get; set; }
    public bool Bloquant { get; set; }
}