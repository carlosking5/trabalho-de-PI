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
    Cancelado = 2,
    Esgotado = 3,   // outro cliente pagou primeiro e os lugares que sobram já não chegam
    Expirado = 4    // passaram mais de 10 minutos sem pagamento
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

    // Uma reserva pendente tem 10 minutos para ser paga; depois passa a Expirado
    public static readonly TimeSpan PrazoPagamento = TimeSpan.FromMinutes(10);
    public DateTime PagarAte => DataReserva + PrazoPagamento;

    // Pendente mas já fora do prazo (ainda não foi marcada como Expirado na base de dados)
    public bool PrazoTerminou => EstadoPagamento == EstadoPagamento.Pendente && DateTime.Now >= PagarAte;

    // Ainda se pode pagar: pendente, dentro do prazo, não cancelada e a sessão ainda não começou
    public bool PodePagar => EstadoPagamento == EstadoPagamento.Pendente
                             && !PrazoTerminou
                             && Estado == EstadoReserva.Ativa
                             && Sessao is { JaPassou: false };

    // O cliente pode cancelar uma reserva ativa (paga ou ainda pendente) antes de a sessão começar.
    // As esgotadas e expiradas já não precisam: não seguram lugar nem foram pagas.
    public bool PodeCancelar => Estado == EstadoReserva.Ativa
                                && EstadoPagamento is EstadoPagamento.Pago or EstadoPagamento.Pendente
                                && !PrazoTerminou
                                && Sessao is { JaPassou: false };

    // Total = quantidade x preço da sessão (precisa da Sessao carregada)
    public decimal Total => Quantidade * (Sessao?.Preco ?? 0);
}
