using System.ComponentModel.DataAnnotations;
using CineReserva.Models;

namespace CineReserva.ViewModels;

// Formulário de adicionar/editar filme (área do administrador).
// Ao ADICIONAR, o formulário também cria a primeira sessão (data, hora, preço, lugares).
public class FilmeFormViewModel : IValidatableObject
{
    public int Id { get; set; }
    public bool EhNovo => Id == 0;

    // ----- Campos para ESCREVER -----
    [Display(Name = "Título")]
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, ErrorMessage = "O título pode ter no máximo 150 caracteres.")]
    public string Titulo { get; set; } = "";

    [Display(Name = "Sinopse")]
    [Required(ErrorMessage = "A sinopse é obrigatória.")]
    [StringLength(2000, ErrorMessage = "A sinopse pode ter no máximo 2000 caracteres.")]
    public string Sinopse { get; set; } = "";

    [Display(Name = "Duração (minutos)")]
    [Required(ErrorMessage = "A duração é obrigatória.")]
    [Range(1, 600, ErrorMessage = "A duração tem de estar entre 1 e 600 minutos.")]
    public int? Duracao { get; set; }

    [Display(Name = "Link do trailer do YouTube (opcional)")]
    [StringLength(300)]
    public string? LinkTrailer { get; set; }

    // ----- Campos para SELECIONAR -----
    [Display(Name = "Tipo")]
    public TipoConteudo Tipo { get; set; }

    [Display(Name = "Categoria")]
    [Required(ErrorMessage = "Escolha uma categoria.")]
    public int? CategoriaId { get; set; }

    [Display(Name = "Classificação etária")]
    [Required(ErrorMessage = "Escolha a classificação etária.")]
    public string Classificacao { get; set; } = "Todos";

    // ----- Cartaz -----
    [Display(Name = "Cartaz")]
    public IFormFile? FicheiroCartaz { get; set; }

    // Ao editar, guarda o caminho do cartaz que já existe
    public string? CartazAtual { get; set; }

    // ----- Primeira sessão (só ao adicionar) -----
    [Display(Name = "Data")]
    [DataType(DataType.Date)]
    public DateTime? Data { get; set; }

    [Display(Name = "Hora")]
    [DataType(DataType.Time)]
    public DateTime? Hora { get; set; }

    [Display(Name = "Preço do bilhete (€)")]
    public decimal? Preco { get; set; }

    [Display(Name = "Número de lugares")]
    public int? Lotacao { get; set; }

    // Validações que dependem de vários campos
    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (!string.IsNullOrWhiteSpace(LinkTrailer) &&
            !(LinkTrailer.Contains("youtube.com/") || LinkTrailer.Contains("youtu.be/")))
        {
            yield return new ValidationResult("O link tem de ser do YouTube.", new[] { nameof(LinkTrailer) });
        }

        if (!EhNovo) yield break; // ao editar, as sessões gerem-se na página "Sessões"

        if (Data is null)
            yield return new ValidationResult("Escolha a data da sessão.", new[] { nameof(Data) });
        if (Hora is null)
            yield return new ValidationResult("Escolha a hora da sessão.", new[] { nameof(Hora) });
        if (Data is not null && Hora is not null && Data.Value.Date + Hora.Value.TimeOfDay <= DateTime.Now)
            yield return new ValidationResult("A sessão tem de ser no futuro.", new[] { nameof(Data) });

        if (Preco is null || Preco <= 0 || Preco > 1000)
            yield return new ValidationResult("Indique um preço entre 0,01 € e 1000 €.", new[] { nameof(Preco) });

        if (Lotacao is null || Lotacao < 1 || Lotacao > 1000)
            yield return new ValidationResult("O número de lugares tem de estar entre 1 e 1000.", new[] { nameof(Lotacao) });
    }
}
