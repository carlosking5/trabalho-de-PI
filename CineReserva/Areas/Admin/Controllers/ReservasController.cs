using CineReserva.Data;
using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class ReservasController : Controller
{
    private readonly CinemaDbContext _db;

    public ReservasController(CinemaDbContext db) => _db = db;

    // /Admin/Reservas?filmeId=2&data=2026-11-24
    public async Task<IActionResult> Index(int? filmeId, DateTime? data)
    {
        var consulta = _db.Reservas
            .AsNoTracking()
            .Include(r => r.Utilizador)
            .Include(r => r.Sessao).ThenInclude(s => s!.Filme)
            .AsQueryable();

        if (filmeId is not null)
            consulta = consulta.Where(r => r.Sessao!.FilmeId == filmeId);

        // Filtra pelo dia da sessão
        if (data is not null)
        {
            var inicio = data.Value.Date;
            var fim = inicio.AddDays(1);
            consulta = consulta.Where(r => r.Sessao!.DataHora >= inicio && r.Sessao.DataHora < fim);
        }

        var vm = new ReservasAdminViewModel
        {
            Reservas = await consulta.OrderByDescending(r => r.DataReserva).ToListAsync(),
            Filmes = await _db.Filmes.AsNoTracking().OrderBy(f => f.Titulo).ToListAsync(),
            FilmeId = filmeId,
            Data = data
        };

        return View(vm);
    }
}
