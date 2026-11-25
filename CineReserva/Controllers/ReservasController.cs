using CineReserva.Data;
using CineReserva.Models;
using CineReserva.Servicos;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Controllers;

// [Authorize] = é preciso ter sessão iniciada. Se não tiver, o site manda para /Conta/Entrar.
[Authorize]
public class ReservasController : Controller
{
    private readonly CinemaDbContext _db;
    private readonly UserManager<Utilizador> _utilizadores;

    public ReservasController(CinemaDbContext db, UserManager<Utilizador> utilizadores)
    {
        _db = db;
        _utilizadores = utilizadores;
    }

    private string UtilizadorId => _utilizadores.GetUserId(User)!;

    // /Reservas  ->  "Minhas Reservas"
    public async Task<IActionResult> Index()
    {
        await RegrasReservas.ExpirarPendentesAsync(_db);

        var reservas = await _db.Reservas
            .AsNoTracking()
            .Include(r => r.Sessao).ThenInclude(s => s!.Filme)
            .Where(r => r.UtilizadorId == UtilizadorId)
            .OrderByDescending(r => r.DataReserva)
            .ToListAsync();

        return View(reservas);
    }

    // GET /Reservas/Criar?sessaoId=3  ->  página com o resumo e o seletor de quantidade
    public async Task<IActionResult> Criar(int sessaoId)
    {
        var sessao = await CarregarSessao(sessaoId);
        if (sessao is null) return NotFound();

        var erro = VerificarSessao(sessao);
        if (erro is not null)
        {
            TempData["Erro"] = erro;
            return RedirectToAction("Detalhes", "Filmes", new { id = sessao.FilmeId });
        }

        return View(new CriarReservaViewModel { SessaoId = sessaoId, Sessao = sessao });
    }

    // POST /Reservas/Criar  ->  botão "Pagar reserva"
    // Cria a reserva com o pagamento PENDENTE e abre o ecrã de pagamento.
    // Os lugares só ficam vendidos quando o pagamento for confirmado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarReservaViewModel vm)
    {
        var sessao = await CarregarSessao(vm.SessaoId);
        if (sessao is null) return NotFound();

        var erro = VerificarSessao(sessao);
        if (erro is not null)
        {
            TempData["Erro"] = erro;
            return RedirectToAction("Detalhes", "Filmes", new { id = sessao.FilmeId });
        }

        if (vm.Quantidade > sessao.LugaresRestantes)
            ModelState.AddModelError(nameof(vm.Quantidade),
                $"Só restam {sessao.LugaresRestantes} bilhetes para esta sessão.");

        if (!ModelState.IsValid)
        {
            vm.Sessao = sessao;
            return View(vm);
        }

        // Gera um código que ainda não exista
        string codigo;
        do { codigo = GeradorCodigo.Novo(); }
        while (await _db.Reservas.AnyAsync(r => r.Codigo == codigo));

        var reserva = new Reserva
        {
            UtilizadorId = UtilizadorId,
            SessaoId = sessao.Id,
            Quantidade = vm.Quantidade,
            Codigo = codigo,
            DataReserva = DateTime.Now,
            Estado = EstadoReserva.Ativa,
            EstadoPagamento = EstadoPagamento.Pendente
        };

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Pagamento), new { id = reserva.Id });
    }

    // GET /Reservas/Pagamento/7  ->  escolher Cartão ou MB WAY, com o resumo e o total
    public async Task<IActionResult> Pagamento(int id)
    {
        var reserva = await CarregarReserva(id);
        if (reserva is null) return NotFound();

        var desvio = DesvioSeNaoPodePagar(reserva);
        if (desvio is not null) return desvio;

        return View(new PagamentoViewModel
        {
            ReservaId = reserva.Id,
            Metodo = reserva.MetodoPagamento, // se voltou do ecrã de espera, fica escolhido o mesmo método
            Reserva = reserva
        });
    }

    // POST /Reservas/Pagamento/7  ->  valida os dados e passa para o ecrã de espera
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagamento(int id, PagamentoViewModel vm)
    {
        var reserva = await CarregarReserva(id);
        if (reserva is null) return NotFound();

        var desvio = DesvioSeNaoPodePagar(reserva);
        if (desvio is not null) return desvio;

        if (!ModelState.IsValid)
        {
            vm.ReservaId = id;
            vm.Reserva = reserva;
            vm.Cvv = null;                  // por segurança o CVV nunca é devolvido à página
            ModelState.Remove(nameof(vm.Cvv));
            return View(vm);
        }

        // Grava SÓ o método. Os dados do cartão / telemóvel são descartados aqui.
        reserva.MetodoPagamento = vm.Metodo;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Aguardar), new { id });
    }

    // GET /Reservas/Aguardar/7  ->  "Confirme o pagamento na app ..."
    public async Task<IActionResult> Aguardar(int id)
    {
        var reserva = await CarregarReserva(id);
        if (reserva is null) return NotFound();

        var desvio = DesvioSeNaoPodePagar(reserva);
        if (desvio is not null) return desvio;

        // Ainda não preencheu os dados de pagamento
        if (reserva.MetodoPagamento is null) return RedirectToAction(nameof(Pagamento), new { id });

        return View(reserva);
    }

    // POST /Reservas/ConfirmarPagamento/7  ->  botão "Já autorizei o pagamento"
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarPagamento(int id)
    {
        // A transação garante que dois clientes ao mesmo tempo não ficam os dois com o último bilhete
        // (no SQLite, BeginTransaction bloqueia a escrita até ao Commit).
        await using var transacao = await _db.Database.BeginTransactionAsync();

        await RegrasReservas.ExpirarPendentesAsync(_db);

        var reserva = await _db.Reservas
            .Include(r => r.Sessao).ThenInclude(s => s!.Reservas)
            .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == UtilizadorId);
        if (reserva is null) return NotFound();

        var desvio = DesvioSeNaoPodePagar(reserva);
        if (desvio is not null) return desvio;

        if (reserva.MetodoPagamento is null) return RedirectToAction(nameof(Pagamento), new { id });

        // As pendentes não seguram lugar: verifica OUTRA VEZ se ainda há bilhetes suficientes.
        // Se não houver, não confirma e a reserva passa a Esgotado.
        if (reserva.Quantidade > reserva.Sessao!.LugaresRestantes)
        {
            reserva.EstadoPagamento = EstadoPagamento.Esgotado;
            await _db.SaveChangesAsync();
            await transacao.CommitAsync();

            TempData["Erro"] = RegrasReservas.MensagemEsgotado;
            return RedirectToAction(nameof(Index));
        }

        reserva.EstadoPagamento = EstadoPagamento.Pago;

        // As outras pendentes desta sessão que já não cabem nos lugares que sobram passam a Esgotado
        RegrasReservas.MarcarPendentesSemLugar(reserva.Sessao);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        return RedirectToAction(nameof(Confirmacao), new { id });
    }

    // POST /Reservas/CancelarPagamento/7  ->  botão "Cancelar pagamento"
    // A reserva continua PENDENTE e o cliente volta ao ecrã de pagamento.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarPagamento(int id)
    {
        var reserva = await CarregarReserva(id);
        if (reserva is null) return NotFound();

        var desvio = DesvioSeNaoPodePagar(reserva);
        if (desvio is not null) return desvio;

        TempData["Aviso"] = "Pagamento cancelado. A reserva continua pendente: pode pagar quando quiser.";
        return RedirectToAction(nameof(Pagamento), new { id });
    }

    // /Reservas/Confirmacao/7  ->  o bilhete, com o código da reserva (só depois de pago)
    public async Task<IActionResult> Confirmacao(int id)
    {
        var reserva = await CarregarReserva(id);
        if (reserva is null) return NotFound();

        if (reserva.EstadoPagamento != EstadoPagamento.Pago)
            return RedirectToAction(nameof(Pagamento), new { id });

        return View(reserva);
    }

    // POST /Reservas/Cancelar/7  ->  os bilhetes voltam a ficar disponíveis
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await RegrasReservas.ExpirarPendentesAsync(_db);

        var reserva = await _db.Reservas
            .Include(r => r.Sessao)
            .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == UtilizadorId);

        if (reserva is null) return NotFound();

        if (!reserva.PodeCancelar)
        {
            TempData["Erro"] = "Esta reserva já não pode ser cancelada.";
        }
        else
        {
            reserva.Estado = EstadoReserva.Cancelada;
            reserva.EstadoPagamento = EstadoPagamento.Cancelado;
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = $"A reserva {reserva.Codigo} foi cancelada.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ----- Funções auxiliares -----

    // Reserva do cliente com sessão e filme (outro cliente não consegue abrir: dá 404).
    // Antes de a ler, passa a Expirado as pendentes com mais de 10 minutos.
    private async Task<Reserva?> CarregarReserva(int id)
    {
        await RegrasReservas.ExpirarPendentesAsync(_db);
        return await _db.Reservas
            .Include(r => r.Sessao).ThenInclude(s => s!.Filme)
            .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == UtilizadorId);
    }

    // Se a reserva já não puder ser paga, diz para onde mandar o cliente; senão devolve null
    private IActionResult? DesvioSeNaoPodePagar(Reserva reserva)
    {
        if (reserva.PodePagar) return null;

        if (reserva.EstadoPagamento == EstadoPagamento.Pago)
            return RedirectToAction(nameof(Confirmacao), new { id = reserva.Id });

        TempData["Erro"] = reserva switch
        {
            { Estado: EstadoReserva.Cancelada } => "Esta reserva foi cancelada.",
            { EstadoPagamento: EstadoPagamento.Esgotado } => RegrasReservas.MensagemEsgotado,
            { EstadoPagamento: EstadoPagamento.Expirado } or { PrazoTerminou: true } => RegrasReservas.MensagemExpirado,
            _ => "Esta sessão já começou. Já não é possível pagar esta reserva."
        };
        return RedirectToAction(nameof(Index));
    }

    private Task<Sessao?> CarregarSessao(int id) => _db.Sessoes
        .Include(s => s.Filme).ThenInclude(f => f!.Categorias)
        .Include(s => s.Reservas)
        .FirstOrDefaultAsync(s => s.Id == id);

    // Regras do enunciado: só sessões futuras e não esgotadas
    private static string? VerificarSessao(Sessao sessao)
    {
        if (sessao.JaPassou) return "Esta sessão já começou. Só é possível reservar sessões futuras.";
        if (sessao.Esgotada) return "Esta sessão está esgotada.";
        return null;
    }
}
