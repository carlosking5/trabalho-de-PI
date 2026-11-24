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

    // POST /Reservas/Criar  ->  botão "Confirmar reserva"
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarReservaViewModel vm)
    {
        // A transação garante que dois clientes ao mesmo tempo não compram o último bilhete os dois
        // (no SQLite, BeginTransaction bloqueia a escrita até ao Commit).
        await using var transacao = await _db.Database.BeginTransactionAsync();

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
            Estado = EstadoReserva.Ativa
        };

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        return RedirectToAction(nameof(Confirmacao), new { id = reserva.Id });
    }

    // /Reservas/Confirmacao/7  ->  mostra o código da reserva
    public async Task<IActionResult> Confirmacao(int id)
    {
        var reserva = await _db.Reservas
            .AsNoTracking()
            .Include(r => r.Sessao).ThenInclude(s => s!.Filme)
            .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == UtilizadorId);

        if (reserva is null) return NotFound();
        return View(reserva);
    }

    // POST /Reservas/Cancelar/7  ->  os bilhetes voltam a ficar disponíveis
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var reserva = await _db.Reservas
            .Include(r => r.Sessao)
            .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == UtilizadorId);

        if (reserva is null) return NotFound();

        if (reserva.Estado != EstadoReserva.Ativa || reserva.Sessao!.JaPassou)
        {
            TempData["Erro"] = "Esta reserva já não pode ser cancelada.";
        }
        else
        {
            reserva.Estado = EstadoReserva.Cancelada;
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = $"A reserva {reserva.Codigo} foi cancelada.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ----- Funções auxiliares -----

    private Task<Sessao?> CarregarSessao(int id) => _db.Sessoes
        .Include(s => s.Filme).ThenInclude(f => f!.Categoria)
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
