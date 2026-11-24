using CineReserva.Data;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Controllers;

public class HomeController : Controller
{
    private readonly CinemaDbContext _db;

    public HomeController(CinemaDbContext db) => _db = db;

    // Página inicial: banner com filme em destaque, "Em cartaz" e "Brevemente"
    public async Task<IActionResult> Index()
    {
        var filmes = await _db.Filmes
            .AsNoTracking()
            .Include(f => f.Categoria)
            .Include(f => f.Sessoes).ThenInclude(s => s.Reservas)
            .ToListAsync();

        var cartoes = filmes.Select(f => new FilmeCartao(f)).Where(c => c.Visivel).ToList();

        var emCartaz = cartoes
            .Where(c => c.EmCartaz)
            .OrderBy(c => c.ProximaSessao!.DataHora)
            .ToList();

        var vm = new InicioViewModel
        {
            EmCartaz = emCartaz,
            Brevemente = cartoes
                .Where(c => c.Brevemente)
                .OrderBy(c => c.ProximaSessao?.DataHora ?? DateTime.MaxValue)
                .ToList(),
            // Destaque: o próximo filme em cartaz que ainda tem bilhetes
            Destaque = emCartaz.FirstOrDefault(c => !c.Esgotado) ?? emCartaz.FirstOrDefault(),
            Categorias = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync()
        };

        return View(vm);
    }

    public IActionResult Erro() => View();
}
