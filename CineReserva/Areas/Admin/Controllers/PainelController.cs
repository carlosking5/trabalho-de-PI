using CineReserva.Data;
using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class PainelController : Controller
{
    private readonly CinemaDbContext _db;

    public PainelController(CinemaDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var hoje = DateTime.Today;
        var amanha = hoje.AddDays(1);
        var agora = DateTime.Now;

        // Reservas que contam como venda (as canceladas não contam)
        var vendas = await _db.Reservas
            .AsNoTracking()
            .Include(r => r.Sessao)
            .Where(r => r.Estado != EstadoReserva.Cancelada)
            .ToListAsync();

        var vm = new PainelViewModel
        {
            BilhetesHoje = vendas
                .Where(r => r.DataReserva >= hoje && r.DataReserva < amanha)
                .Sum(r => r.Quantidade),

            TotalFaturado = vendas.Sum(r => r.Total),

            SessoesHoje = await _db.Sessoes
                .CountAsync(s => s.DataHora >= hoje && s.DataHora < amanha),

            // Filmes com pelo menos uma sessão nos próximos 7 dias
            FilmesEmCartaz = await _db.Sessoes
                .Where(s => s.DataHora > agora && s.DataHora <= agora.AddDays(FilmeCartao.DiasEmCartaz))
                .Select(s => s.FilmeId)
                .Distinct()
                .CountAsync(),

            ProximasSessoes = await _db.Sessoes
                .AsNoTracking()
                .Include(s => s.Filme)
                .Include(s => s.Reservas)
                .Where(s => s.DataHora > agora)
                .OrderBy(s => s.DataHora)
                .Take(8)
                .ToListAsync()
        };

        return View(vm);
    }
}
