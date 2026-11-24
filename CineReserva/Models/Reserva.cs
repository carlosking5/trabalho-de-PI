namespace CineReserva.Models;

public enum EstadoReserva
{
    Ativa = 0,
    Usada = 1,
    Cancelada = 2
}

// Tabela "Reservas": um cliente compra X bilhetes para uma sessão.
// Não há escolha de cadeira: o cliente senta-se onde quiser.
public class Reserva
{
    public int Id { get; set; }

    public string UtilizadorId { get; set; } = "";
    public Utilizador? Utilizador { get; set; }

    public int SessaoId { get; set; }
    public Sessao? Sessao { get; set; }

    public int Quantidade { get; set; }

    // Código único que o cliente mostra à entrada (ex: "K7P2QX9M")
    public string Codigo { get; set; } = "";

    public DateTime DataReserva { get; set; }

    public EstadoReserva Estado { get; set; } = EstadoReserva.Ativa;

    // Total = quantidade x preço da sessão (precisa da Sessao carregada)
    public decimal Total => Quantidade * (Sessao?.Preco ?? 0);
}
