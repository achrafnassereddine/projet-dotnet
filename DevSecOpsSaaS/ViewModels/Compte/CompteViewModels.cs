using System.ComponentModel.DataAnnotations;

namespace DevSecOpsSaaS.ViewModels.Compte;

public class InscriptionOrganisationViewModel
{
    [Required(ErrorMessage = "Le nom de l'organisation est obligatoire.")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Nom de l'organisation")]
    public string NomOrganisation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prenom est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Prenom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    [StringLength(256)]
    [Display(Name = "Email professionnel")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Numero de telephone invalide.")]
    [StringLength(30)]
    [Display(Name = "Telephone (optionnel)")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Le mot de passe doit contenir au moins 10 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
    [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmationMotDePasse { get; set; } = string.Empty;
}

public class ConnexionViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress]
    [Display(Name = "Adresse email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class AccepterInvitationViewModel
{
    [Required]
    public string Jeton { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NomOrganisation { get; set; } = string.Empty;

    public string LibelleRole { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prenom est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Prenom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    [Display(Name = "Telephone (optionnel)")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Minimum 10 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmationMotDePasse { get; set; } = string.Empty;
}