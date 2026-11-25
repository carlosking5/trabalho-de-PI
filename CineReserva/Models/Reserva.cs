using System.ComponentModel.DataAnnotations;

namespace CineReserva.Models;

public enum EstadoReserva
{
    Ativa = 0,
    Usada = 1,
    Cancelada = 2
}

// Como o cliente pagou (o pagamento é só uma simulação)
public enum MetodoPagamento
{
    [Display(Name = "Cartão")]
    Cartao = 0,
    [Display(Name = "MB WAY")]
    MbWay = 1
}

public enum EstadoPagamento
{
    Pendente = 0,
    Pago = 1,
    Cancelado = 2
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

    // ----- Pagamento -----
    // Só se guarda o método e o estado. O número do cartão, o CVV e o telemóvel
    // servem apenas para validar o formulário e NUNCA são gravados.
    public MetodoPagamento? MetodoPagamento { get; set; }   // fica NULL até o cliente escolher
    public EstadoPagamento EstadoPagamento { get; set; } = EstadoPagamento.Pendente;

    // Só uma reserva paga (e não cancelada) ocupa lugares e conta no total faturado
    public bool ContaComoVenda => EstadoPagamento == EstadoPagamento.Pago && Estado != EstadoReserva.Cancelada;

    // Ainda se pode pagar: pendente, não cancelada e a sessão ainda não começou
    public bool PodePagar => EstadoPagamento == EstadoPagamento.Pendente
                             && Estado == EstadoReserva.Ativa
                             && Sessao is { JaPassou: false };

    // Total = quantidade x preço da sessão (precisa da Sessao carregada)
    public decimal Total => Quantidade * (Sessao?.Preco ?? 0);
}
