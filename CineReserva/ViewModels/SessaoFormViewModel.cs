using System.ComponentModel.DataAnnotations;

namespace CineReserva.ViewModels;

// Formulário de adicionar/editar sessão (área do administrador)
public class SessaoFormViewModel : IValidatableObject
{
    public int Id { get; set; }
    public bool EhNova => Id == 0;

    [Display(Name = "Filme")]
    [Required(ErrorMessage = "Escolha o filme.")]
    public int? FilmeId { get; set; }

    [Display(Name = "Data")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Escolha a data.")]
    public DateTime? Data { get; set; }

    [Display(Name = "Hora")]
    [DataType(DataType.Time)]
    [Required(ErrorMessage = "Escolha a hora.")]
    public DateTime? Hora { get; set; }

    [Display(Name = "Preço do bilhete (€)")]
    [Required(ErrorMessage = "Indique o preço.")]
    public decimal? Preco { get; set; }

    [Display(Name = "Número de lugares")]
    [Required(ErrorMessage = "Indique o número de lugares.")]
    [Range(1, 1000, ErrorMessage = "O número de lugares tem de estar entre 1 e 1000.")]
    public int? Lotacao { get; set; }

    public DateTime DataHora => (Data ?? DateTime.Today).Date + (Hora ?? DateTime.MinValue).TimeOfDay;

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Preco is not null && (Preco <= 0 || Preco > 1000))
            yield return new ValidationResult("Indique um preço entre 0,01 € e 1000 €.", new[] { nameof(Preco) });

        if (EhNova && Data is not null && Hora is not null && DataHora <= DateTime.Now)
            yield return new ValidationResult("A sessão tem de ser no futuro.", new[] { nameof(Data) });
    }
}
