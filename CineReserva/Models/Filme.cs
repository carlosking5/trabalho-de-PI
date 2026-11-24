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

    // Um filme tem muitas sessões
    public List<Sessao> Sessoes { get; set; } = new();

    // Se não houver cartaz mostra uma imagem genérica
    public string CartazOuPadrao => string.IsNullOrEmpty(Cartaz) ? "/imagens/sem-cartaz.svg" : Cartaz;
}
