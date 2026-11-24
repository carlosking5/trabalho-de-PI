using System.ComponentModel.DataAnnotations;

namespace CineReserva.ViewModels;

public class EntrarViewModel
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O email não é válido.")]
    public string Email { get; set; } = "";

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = "";

    [Display(Name = "Lembrar-me")]
    public bool Lembrar { get; set; }

    public string? ReturnUrl { get; set; }
}

public class CriarContaViewModel
{
    [Display(Name = "Nome")]
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; } = "";

    [Display(Name = "Email")]
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O email não é válido.")]
    public string Email { get; set; } = "";

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = "";

    [Display(Name = "Confirmar senha")]
    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmarSenha { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
