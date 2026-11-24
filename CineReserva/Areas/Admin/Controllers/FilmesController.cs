using CineReserva.Data;
using CineReserva.Infraestrutura;
using CineReserva.Models;
using CineReserva.Servicos;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class FilmesController : Controller
{
    private readonly CinemaDbContext _db;
    private readonly GestorImagens _imagens;

    public FilmesController(CinemaDbContext db, GestorImagens imagens)
    {
        _db = db;
        _imagens = imagens;
    }

    public async Task<IActionResult> Index()
    {
        var filmes = await _db.Filmes
            .AsNoTracking()
            .Include(f => f.Categoria)
            .Include(f => f.Sessoes)
            .OrderBy(f => f.Titulo)
            .ToListAsync();
        return View(filmes);
    }

    // Botão "ver"
    public async Task<IActionResult> Detalhes(int id)
    {
        var filme = await _db.Filmes
            .AsNoTracking()
            .Include(f => f.Categoria)
            .Include(f => f.Sessoes).ThenInclude(s => s.Reservas)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (filme is null) return NotFound();
        return View(filme);
    }

    public async Task<IActionResult> Criar()
    {
        await PreencherListas();
        // Sugere amanhã às 21:00 como primeira sessão
        var vm = new FilmeFormViewModel
        {
            Data = DateTime.Today.AddDays(1),
            Hora = DateTime.Today.AddHours(21)
        };
        return View("Formulario", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(FilmeFormViewModel vm)
    {
        await ValidarCategoriaECartaz(vm);
        if (!ModelState.IsValid)
        {
            await PreencherListas();
            return View("Formulario", vm);
        }

        var filme = new Filme();
        CopiarCampos(vm, filme);

        // Grava a imagem escolhida em wwwroot/imagens
        if (vm.FicheiroCartaz is { Length: > 0 })
            filme.Cartaz = await _imagens.GuardarAsync(vm.FicheiroCartaz);

        // Primeira sessão do filme (Lotacao = nº de lugares = nº de bilhetes à venda)
        filme.Sessoes.Add(new Sessao
        {
            DataHora = vm.Data!.Value.Date + vm.Hora!.Value.TimeOfDay,
            Preco = vm.Preco!.Value,
            Lotacao = vm.Lotacao!.Value
        });

        _db.Filmes.Add(filme);
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = $"\"{filme.Titulo}\" foi adicionado com a primeira sessão.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var filme = await _db.Filmes.FindAsync(id);
        if (filme is null) return NotFound();

        var vm = new FilmeFormViewModel
        {
            Id = filme.Id,
            Titulo = filme.Titulo,
            Tipo = filme.Tipo,
            CategoriaId = filme.CategoriaId,
            Classificacao = filme.Classificacao,
            Sinopse = filme.Sinopse,
            Duracao = filme.Duracao,
            LinkTrailer = filme.LinkTrailer,
            CartazAtual = filme.Cartaz,
            Temporada = filme.Temporada,
            EpisodioInicial = filme.EpisodioInicial,
            EpisodioFinal = filme.EpisodioFinal
        };

        await PreencherListas();
        return View("Formulario", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, FilmeFormViewModel vm)
    {
        var filme = await _db.Filmes.FindAsync(id);
        if (filme is null) return NotFound();

        vm.Id = id;
        vm.CartazAtual = filme.Cartaz;

        await ValidarCategoriaECartaz(vm);
        if (!ModelState.IsValid)
        {
            await PreencherListas();
            return View("Formulario", vm);
        }

        CopiarCampos(vm, filme);

        // Se escolheu um novo cartaz, grava o novo e apaga o antigo
        if (vm.FicheiroCartaz is { Length: > 0 })
        {
            var antigo = filme.Cartaz;
            filme.Cartaz = await _imagens.GuardarAsync(vm.FicheiroCartaz);
            _imagens.Apagar(antigo);
        }

        await _db.SaveChangesAsync();

        TempData["Sucesso"] = $"\"{filme.Titulo}\" foi atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apagar(int id)
    {
        var filme = await _db.Filmes.FindAsync(id);
        if (filme is null) return NotFound();

        // As sessões e reservas deste filme são apagadas em cascata (ver CinemaDbContext)
        _db.Filmes.Remove(filme);
        await _db.SaveChangesAsync();
        _imagens.Apagar(filme.Cartaz);

        TempData["Sucesso"] = $"\"{filme.Titulo}\" foi apagado.";
        return RedirectToAction(nameof(Index));
    }

    // ----- Funções auxiliares -----

    private static void CopiarCampos(FilmeFormViewModel vm, Filme filme)
    {
        filme.Titulo = vm.Titulo.Trim();
        filme.Tipo = vm.Tipo;
        filme.CategoriaId = vm.CategoriaId!.Value;
        filme.Classificacao = vm.Classificacao;
        filme.Sinopse = vm.Sinopse.Trim();
        filme.Duracao = vm.Duracao!.Value;
        filme.LinkTrailer = string.IsNullOrWhiteSpace(vm.LinkTrailer) ? null : vm.LinkTrailer.Trim();

        // Temporada e episódios só existem nas séries; nos filmes ficam NULL
        var serie = vm.Tipo == TipoConteudo.Serie;
        filme.Temporada = serie ? vm.Temporada : null;
        filme.EpisodioInicial = serie ? vm.EpisodioInicial : null;
        filme.EpisodioFinal = serie ? vm.EpisodioFinal : null;
    }

    private async Task ValidarCategoriaECartaz(FilmeFormViewModel vm)
    {
        if (vm.CategoriaId is not null && !await _db.Categorias.AnyAsync(c => c.Id == vm.CategoriaId))
            ModelState.AddModelError(nameof(vm.CategoriaId), "Categoria inválida.");

        if (!Filme.Classificacoes.Contains(vm.Classificacao))
            ModelState.AddModelError(nameof(vm.Classificacao), "Classificação inválida.");

        // Séries: temporada e episódios obrigatórios, final >= inicial, máximo 3 por sessão
        foreach (var (campo, mensagem) in vm.ErrosSerie())
            ModelState.AddModelError(campo, mensagem);

        if (vm.FicheiroCartaz is { Length: > 0 })
        {
            var erro = _imagens.Validar(vm.FicheiroCartaz);
            if (erro is not null) ModelState.AddModelError(nameof(vm.FicheiroCartaz), erro);
        }
    }

    // Listas de seleção do formulário (nada escrito à mão)
    private async Task PreencherListas()
    {
        var categorias = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
        ViewBag.Categorias = new SelectList(categorias, nameof(Categoria.Id), nameof(Categoria.Nome));

        ViewBag.Tipos = Enum.GetValues<TipoConteudo>()
            .Select(t => new SelectListItem(t.Nome(), t.ToString()))
            .ToList();

        ViewBag.Classificacoes = new SelectList(Filme.Classificacoes);
    }
}
