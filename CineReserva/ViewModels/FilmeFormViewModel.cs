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

    // ----- Só para séries (aparecem com JavaScript quando o Tipo é "Série") -----
    [Display(Name = "Temporada")]
    public int? Temporada { get; set; }

    [Display(Name = "Episódio inicial")]
    public int? EpisodioInicial { get; set; }

    [Display(Name = "Episódio final")]
    public int? EpisodioFinal { get; set; }

    // Regras das séries. É chamado pelo controller para correr SEMPRE,
    // mesmo que haja outros erros no formulário.
    public IEnumerable<(string Campo, string Mensagem)> ErrosSerie()
    {
        if (Tipo != TipoConteudo.Serie) yield break; // nos filmes estes campos não são obrigatórios

        if (Temporada is null || Temporada < 1)
            yield return (nameof(Temporada), "Indique a temporada (número maior que 0).");
        if (EpisodioInicial is null || EpisodioInicial < 1)
            yield return (nameof(EpisodioInicial), "Indique o episódio inicial (número maior que 0).");
        if (EpisodioFinal is null || EpisodioFinal < 1)
            yield return (nameof(EpisodioFinal), "Indique o episódio final (número maior que 0).");

        if (EpisodioInicial >= 1 && EpisodioFinal >= 1)
        {
            if (EpisodioFinal < EpisodioInicial)
                yield return (nameof(EpisodioFinal), "O episódio final tem de ser maior ou igual ao inicial.");
            else if (EpisodioFinal - EpisodioInicial + 1 > Filme.MaxEpisodiosPorSessao)
                yield return (nameof(EpisodioFinal), "Uma sessão pode ter no máximo 3 episódios.");
        }
    }

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
