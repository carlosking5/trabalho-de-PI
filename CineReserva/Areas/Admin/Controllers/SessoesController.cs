using CineReserva.Data;
using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class SessoesController : Controller
{
    private readonly CinemaDbContext _db;

    public SessoesController(CinemaDbContext db) => _db = db;

    // /Admin/Sessoes            -> sessões futuras
    // /Admin/Sessoes?passadas=true -> sessões que já passaram
    public async Task<IActionResult> Index(bool passadas = false)
    {
        var agora = DateTime.Now;
        var consulta = _db.Sessoes
            .AsNoTracking()
            .Include(s => s.Filme)
            .Include(s => s.Reservas);

        var sessoes = passadas
            ? await consulta.Where(s => s.DataHora <= agora).OrderByDescending(s => s.DataHora).ToListAsync()
            : await consulta.Where(s => s.DataHora > agora).OrderBy(s => s.DataHora).ToListAsync();

        ViewBag.Passadas = passadas;
        return View(sessoes);
    }

    public async Task<IActionResult> Criar(int? filmeId)
    {
        await PreencherFilmes();
        return View("Formulario", new SessaoFormViewModel
        {
            FilmeId = filmeId,
            Data = DateTime.Today.AddDays(1),
            Hora = DateTime.Today.AddHours(21),
            Lotacao = 40
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(SessaoFormViewModel vm)
    {
        if (vm.FilmeId is not null && !await _db.Filmes.AnyAsync(f => f.Id == vm.FilmeId))
            ModelState.AddModelError(nameof(vm.FilmeId), "Filme inválido.");

        if (!ModelState.IsValid)
        {
            await PreencherFilmes();
            return View("Formulario", vm);
        }

        _db.Sessoes.Add(new Sessao
        {
            FilmeId = vm.FilmeId!.Value,
            DataHora = vm.DataHora,
            Preco = vm.Preco!.Value,
            Lotacao = vm.Lotacao!.Value
        });
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = "Sessão criada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var sessao = await _db.Sessoes.FindAsync(id);
        if (sessao is null) return NotFound();

        await PreencherFilmes();
        return View("Formulario", new SessaoFormViewModel
        {
            Id = sessao.Id,
            FilmeId = sessao.FilmeId,
            Data = sessao.DataHora.Date,
            Hora = DateTime.Today + sessao.DataHora.TimeOfDay,
            Preco = sessao.Preco,
            Lotacao = sessao.Lotacao
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, SessaoFormViewModel vm)
    {
        var sessao = await _db.Sessoes.Include(s => s.Reservas).FirstOrDefaultAsync(s => s.Id == id);
        if (sessao is null) return NotFound();
        vm.Id = id;

        if (vm.FilmeId is not null && !await _db.Filmes.AnyAsync(f => f.Id == vm.FilmeId))
            ModelState.AddModelError(nameof(vm.FilmeId), "Filme inválido.");

        // Não se pode reduzir os lugares para menos do que os bilhetes já vendidos
        if (vm.Lotacao is not null && vm.Lotacao < sessao.BilhetesVendidos)
            ModelState.AddModelError(nameof(vm.Lotacao),
                $"Já foram vendidos {sessao.BilhetesVendidos} bilhetes. O número de lugares não pode ser menor.");

        if (!ModelState.IsValid)
        {
            await PreencherFilmes();
            return View("Formulario", vm);
        }

        sessao.FilmeId = vm.FilmeId!.Value;
        sessao.DataHora = vm.DataHora;
        sessao.Preco = vm.Preco!.Value;
        sessao.Lotacao = vm.Lotacao!.Value;
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = "Sessão atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apagar(int id)
    {
        var sessao = await _db.Sessoes.FindAsync(id);
        if (sessao is null) return NotFound();

        _db.Sessoes.Remove(sessao); // as reservas desta sessão são apagadas em cascata
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = "Sessão apagada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PreencherFilmes()
    {
        var filmes = await _db.Filmes.AsNoTracking().OrderBy(f => f.Titulo).ToListAsync();
        ViewBag.Filmes = new SelectList(filmes, nameof(Filme.Id), nameof(Filme.Titulo));
    }
}
