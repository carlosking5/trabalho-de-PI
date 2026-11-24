using CineReserva.Models;

namespace CineReserva.ViewModels;

// Painel inicial do administrador
public class PainelViewModel
{
    public int BilhetesHoje { get; set; }
    public decimal TotalFaturado { get; set; }
    public int SessoesHoje { get; set; }
    public int FilmesEmCartaz { get; set; }
    public List<Sessao> ProximasSessoes { get; set; } = new();
}

// Lista de reservas com filtros
public class ReservasAdminViewModel
{
    public List<Reserva> Reservas { get; set; } = new();
    public List<Filme> Filmes { get; set; } = new();
    public int? FilmeId { get; set; }
    public DateTime? Data { get; set; }
}

// Resultado da validação de um código à entrada
public class ValidarEntradaViewModel
{
    public string? Codigo { get; set; }
    public bool? Valido { get; set; }      // null = ainda não validou nada
    public string? Mensagem { get; set; }
    public Reserva? Reserva { get; set; }
}

// Relatórios
public class LinhaRelatorio
{
    public string Nome { get; set; } = "";
    public int Bilhetes { get; set; }
    public decimal Faturado { get; set; }
}

public class RelatoriosViewModel
{
    public List<LinhaRelatorio> VendasPorDia { get; set; } = new();
    public List<LinhaRelatorio> VendasPorFilme { get; set; } = new();
    public LinhaRelatorio? FilmeMaisVisto { get; set; }
    public int TotalBilhetes { get; set; }
    public decimal TotalFaturado { get; set; }
}
