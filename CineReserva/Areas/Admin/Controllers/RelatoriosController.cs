using CineReserva.Data;
using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class RelatoriosController : Controller
{
    private readonly CinemaDbContext _db;

    public RelatoriosController(CinemaDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Só contam as reservas PAGAS e não canceladas (as pendentes ainda não são faturadas).
        // As contas são feitas em C# (LINQ) porque o SQLite não sabe somar valores decimal.
        var vendas = await _db.Reservas
            .AsNoTracking()
            .Include(r => r.Sessao).ThenInclude(s => s!.Filme)
            .Where(r => r.EstadoPagamento == EstadoPagamento.Pago && r.Estado != EstadoReserva.Cancelada)
            .ToListAsync();

        var porDia = vendas
            .GroupBy(r => r.DataReserva.Date)
            .OrderByDescending(g => g.Key)
            .Take(30)
            .Select(g => new LinhaRelatorio
            {
                Nome = g.Key.ToString("dd/MM/yyyy"),
                Bilhetes = g.Sum(r => r.Quantidade),
                Faturado = g.Sum(r => r.Total)
            })
            .ToList();

        var porFilme = vendas
            .GroupBy(r => r.Sessao!.Filme!.Titulo)
            .Select(g => new LinhaRelatorio
            {
                Nome = g.Key,
                Bilhetes = g.Sum(r => r.Quantidade),
                Faturado = g.Sum(r => r.Total)
            })
            .OrderByDescending(l => l.Bilhetes)
            .ToList();

        var vm = new RelatoriosViewModel
        {
            VendasPorDia = porDia,
            VendasPorFilme = porFilme,
            FilmeMaisVisto = porFilme.FirstOrDefault(),
            TotalBilhetes = vendas.Sum(r => r.Quantidade),
            TotalFaturado = vendas.Sum(r => r.Total)
        };

        return View(vm);
    }
}
