using System.ComponentModel.DataAnnotations;

namespace CineReserva.Models;

public enum TipoConteudo
{
    Filme = 0,
    [Display(Name = "Série")]
    Serie = 1
}

// Tabela "Filmes"
public class Filme
{
    // Lista fixa de classificações etárias (usada nas listas de seleção)
    public static readonly string[] Classificacoes = { "Todos", "+12", "+14", "+16", "+18" };

    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Titulo { get; set; } = "";

    public TipoConteudo Tipo { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // Caminho da imagem dentro de wwwroot, por exemplo "/imagens/abc123.jpg"
    [StringLength(300)]
    public string? Cartaz { get; set; }

    [Required, StringLength(2000)]
    public string Sinopse { get; set; } = "";

    // Duração em minutos
    public int Duracao { get; set; }

    [Required, StringLength(10)]
    public string Classificacao { get; set; } = "Todos";

    [StringLength(300)]
    public string? LinkTrailer { get; set; }

    // ----- Só para séries (ficam NULL nos filmes) -----
    // Cada sessão da série exibe estes episódios (no máximo 3).
    // Um bilhete dá acesso a todos os episódios da sessão.
    public int? Temporada { get; set; }
    public int? EpisodioInicial { get; set; }
    public int? EpisodioFinal { get; set; }

    public const int MaxEpisodiosPorSessao = 3;

    // "Temporada 2 · Episódios 1 a 3"  ou  "Temporada 2 · Episódio 1"  (null nos filmes)
    public string? DescricaoEpisodios
    {
        get
        {
            if (Tipo != TipoConteudo.Serie || Temporada is null || EpisodioInicial is null || EpisodioFinal is null)
                return null;

            var episodios = EpisodioInicial == EpisodioFinal
                ? $"Episódio {EpisodioInicial}"
                : $"Episódios {EpisodioInicial} a {EpisodioFinal}";

            return $"Temporada {Temporada} · {episodios}";
        }
    }

    // Um filme tem muitas sessões
    public List<Sessao> Sessoes { get; set; } = new();

    // Se não houver cartaz mostra uma imagem genérica
    public string CartazOuPadrao => string.IsNullOrEmpty(Cartaz) ? "/imagens/sem-cartaz.svg" : Cartaz;
}
