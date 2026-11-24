using CineReserva.Data;
using CineReserva.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class CategoriasController : Controller
{
    private readonly CinemaDbContext _db;

    public CategoriasController(CinemaDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var categorias = await _db.Categorias
            .AsNoTracking()
            .Include(c => c.Filmes)
            .OrderBy(c => c.Nome)
            .ToListAsync();
        return View(categorias);
    }

    public IActionResult Criar() => View("Formulario", new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(Categoria categoria)
    {
        await ValidarNomeRepetido(categoria);
        if (!ModelState.IsValid) return View("Formulario", categoria);

        categoria.Id = 0; // o Id é gerado pela base de dados
        categoria.Nome = categoria.Nome.Trim();
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = $"Categoria \"{categoria.Nome}\" criada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null) return NotFound();
        return View("Formulario", categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Categoria dados)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null) return NotFound();

        dados.Id = id;
        await ValidarNomeRepetido(dados);
        if (!ModelState.IsValid) return View("Formulario", dados);

        categoria.Nome = dados.Nome.Trim();
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = "Categoria atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apagar(int id)
    {
        var categoria = await _db.Categorias.Include(c => c.Filmes).FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null) return NotFound();

        if (categoria.Filmes.Count > 0)
        {
            TempData["Erro"] = $"Não é possível apagar \"{categoria.Nome}\": tem {categoria.Filmes.Count} filme(s) associado(s).";
        }
        else
        {
            _db.Categorias.Remove(categoria);
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = $"Categoria \"{categoria.Nome}\" apagada.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarNomeRepetido(Categoria categoria)
    {
        var nome = (categoria.Nome ?? "").Trim();
        var existe = await _db.Categorias.AnyAsync(c => c.Nome == nome && c.Id != categoria.Id);
        if (existe)
            ModelState.AddModelError(nameof(Categoria.Nome), "Já existe uma categoria com esse nome.");
    }
}
