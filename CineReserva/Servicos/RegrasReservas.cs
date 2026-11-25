using CineReserva.Data;
using CineReserva.Models;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Servicos;

// Regras dos lugares e dos pagamentos pendentes.
//  - Só as reservas PAGAS ocupam lugares (ver Sessao.BilhetesVendidos). As pendentes não seguram lugar.
//  - Uma reserva pendente expira ao fim de 10 minutos sem pagamento (-> Expirado).
//  - Quando alguém paga e os lugares que sobram já não chegam para uma pendente da mesma sessão,
//    essa pendente passa a Esgotado.
public static class RegrasReservas
{
    public const string MensagemEsgotado = "Lamentamos, os bilhetes esgotaram enquanto concluía o pagamento.";
    public const string MensagemExpirado = "Esta reserva expirou: passaram mais de 10 minutos sem pagamento. Faça uma nova reserva.";

    // Passa a Expirado todas as reservas pendentes com mais de 10 minutos.
    // É chamado antes de mostrar listas de reservas e antes de qualquer passo do pagamento,
    // por isso o estado aparece sempre certo sem precisar de um processo à parte.
    public static Task<int> ExpirarPendentesAsync(CinemaDbContext db)
    {
        var limite = DateTime.Now - Reserva.PrazoPagamento;
        return db.Reservas
            .Where(r => r.EstadoPagamento == EstadoPagamento.Pendente
                        && r.Estado == EstadoReserva.Ativa
                        && r.DataReserva <= limite)
            .ExecuteUpdateAsync(definir => definir.SetProperty(r => r.EstadoPagamento, EstadoPagamento.Expirado));
    }

    // Depois de um pagamento confirmado: as pendentes da mesma sessão que já não cabem nos
    // lugares que sobram passam a Esgotado. A sessão tem de vir com as Reservas carregadas
    // (e a reserva acabada de pagar já marcada como Pago).
    public static void MarcarPendentesSemLugar(Sessao sessao)
    {
        var restantes = sessao.LugaresRestantes;
        foreach (var pendente in sessao.Reservas.Where(r => r.EstadoPagamento == EstadoPagamento.Pendente
                                                            && r.Estado == EstadoReserva.Ativa
                                                            && r.Quantidade > restantes))
        {
            pendente.EstadoPagamento = EstadoPagamento.Esgotado;
        }
    }
}
