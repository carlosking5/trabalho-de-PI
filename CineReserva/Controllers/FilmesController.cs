using System.Globalization;
using CineReserva.Data;
using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Controllers;

// Páginas públicas dos filmes (qualquer pessoa pode ver, mesmo sem login)
public class FilmesController : Controller
{
    private readonly CinemaDbContext _db;

    public FilmesController(CinemaDbContext db) => _db = db;

    // /Filmes?pesquisa=batman&categoriaId=1&tipo=Filme
    public async Task<IActionResult> Index(string? pesquisa, int? categoriaId, TipoConteudo? tipo)
    {
        var consulta = _db.Filmes
            .AsNoTracking()
            .Include(f => f.Categoria)
            .Include(f => f.Sessoes).ThenInclude(s => s.Reservas)
            .AsQueryable();

        // Os filtros de categoria e tipo são feitos na base de dados
        if (categoriaId is not null)
            consulta = consulta.Where(f => f.CategoriaId == categoriaId);
        if (tipo is not null)
            consulta = consulta.Where(f => f.Tipo == tipo);

        var filmes = await consulta.OrderBy(f => f.Titulo).ToListAsync();

        // A pesquisa por nome é feita aqui para ignorar maiúsculas e acentos ("acao" encontra "Ação")
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var comparador = CultureInfo.GetCultureInfo("pt-PT").CompareInfo;
            var opcoes = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
            filmes = filmes.Where(f => comparador.IndexOf(f.Titulo, pesquisa.Trim(), opcoes) >= 0).ToList();
        }

        var vm = new CatalogoViewModel
        {
            Filmes = filmes.Select(f => new FilmeCartao(f)).Where(c => c.Visivel).ToList(),
            Categorias = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync(),
            Pesquisa = pesquisa,
            CategoriaId = categoriaId,
            Tipo = tipo
        };

        return View(vm);
    }

    // /Filmes/Detalhes/5
    public async Task<IActionResult> Detalhes(int id)
    {
        var filme = await _db.Filmes
            .AsNoTracking()
            .Include(f => f.Categoria)
            .Include(f => f.Sessoes).ThenInclude(s => s.Reservas)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (filme is null) return NotFound();

        return View(new FilmeCartao(filme));
    }
}
