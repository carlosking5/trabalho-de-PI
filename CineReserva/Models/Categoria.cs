using System.ComponentModel.DataAnnotations;

namespace CineReserva.Models;

// Tabela "Categorias": Ação, Comédia, Drama...
public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome pode ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = "";

    // Uma categoria tem muitos filmes e um filme tem muitas categorias (relação N para N)
    public List<Filme> Filmes { get; set; } = new();
}
