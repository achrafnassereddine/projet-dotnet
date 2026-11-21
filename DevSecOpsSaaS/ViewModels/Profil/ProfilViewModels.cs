using System.ComponentModel.DataAnnotations;

namespace DevSecOpsSaaS.ViewModels.Profil;

public class ProfilViewModel
{
    public string Email { get; set; } = string.Empty;
    public string NomOrganisation { get; set; } = string.Empty;
    public string LibelleRole { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }

    [Required(ErrorMessage = "Le prenom est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Prenom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Numero de telephone invalide.")]
    [StringLength(30)]
    [Display(Name = "Telephone")]
    public string? Telephone { get; set; }
}

public class ChangerMotDePasseViewModel
{
    [Required(ErrorMessage = "Le mot de passe actuel est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe actuel")]
    public string MotDePasseActuel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Minimum 10 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    public string NouveauMotDePasse { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NouveauMotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le nouveau mot de passe")]
    public string ConfirmationNouveauMotDePasse { get; set; } = string.Empty;
}