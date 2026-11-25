namespace CineReserva.Models;

// Tabela "Sessoes": uma exibição de um filme num dia e hora.
// Lotacao = número de lugares da sala = número de bilhetes à venda.
public class Sessao
{
    public int Id { get; set; }

    public int FilmeId { get; set; }
    public Filme? Filme { get; set; }

    public DateTime DataHora { get; set; }

    public decimal Preco { get; set; }

    public int Lotacao { get; set; }

    public List<Reserva> Reservas { get; set; } = new();

    // ---- Propriedades calculadas (não são gravadas na base de dados) ----
    // Para funcionarem, as Reservas têm de ser carregadas com .Include(s => s.Reservas)

    // Bilhetes vendidos = soma das quantidades das reservas PAGAS e não canceladas.
    // As reservas com pagamento pendente não ocupam lugares.
    public int BilhetesVendidos => Reservas
        .Where(r => r.ContaComoVenda)
        .Sum(r => r.Quantidade);

    public int LugaresRestantes => Math.Max(0, Lotacao - BilhetesVendidos);

    public bool Esgotada => LugaresRestantes == 0;

    public bool JaPassou => DataHora <= DateTime.Now;

    // Percentagem de ocupação (0 a 100) para as barras de progresso
    public int Ocupacao => Lotacao == 0 ? 0 : (int)Math.Round(BilhetesVendidos * 100.0 / Lotacao);
}
