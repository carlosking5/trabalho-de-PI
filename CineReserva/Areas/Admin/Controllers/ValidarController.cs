using CineReserva.Data;
using CineReserva.Infraestrutura;
using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Areas.Admin.Controllers;

// "Validar Entrada": o funcionário escreve o código que o cliente mostra à porta da sala
[Area("Admin")]
[Authorize(Roles = Perfis.Administrador)]
public class ValidarController : Controller
{
    private readonly CinemaDbContext _db;

    public ValidarController(CinemaDbContext db) => _db = db;

    public IActionResult Index() => View(new ValidarEntradaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string? codigo)
    {
        var vm = new ValidarEntradaViewModel { Codigo = codigo };
        var limpo = (codigo ?? "").Trim().ToUpperInvariant();

        if (limpo.Length == 0)
        {
            vm.Valido = false;
            vm.Mensagem = "Escreva o código da reserva.";
            return View(vm);
        }

        var reserva = await _db.Reservas
            .Include(r => r.Utilizador)
            .Include(r => r.Sessao).ThenInclude(s => s!.Filme)
            .FirstOrDefaultAsync(r => r.Codigo == limpo);

        vm.Reserva = reserva;

        if (reserva is null)
        {
            vm.Valido = false;
            vm.Mensagem = "Código inválido: não existe nenhuma reserva com este código.";
        }
        else if (reserva.Estado == EstadoReserva.Usada)
        {
            vm.Valido = false;
            vm.Mensagem = "Este bilhete já foi usado.";
        }
        else if (reserva.Estado == EstadoReserva.Cancelada)
        {
            vm.Valido = false;
            vm.Mensagem = "Esta reserva foi cancelada.";
        }
        else if (reserva.EstadoPagamento != EstadoPagamento.Pago)
        {
            vm.Valido = false;
            vm.Mensagem = "O pagamento desta reserva ainda não foi confirmado.";
        }
        else if (reserva.Sessao!.DataHora.Date != DateTime.Today)
        {
            vm.Valido = false;
            vm.Mensagem = $"Esta reserva é para outro dia ({reserva.Sessao.DataHora.DiaHora()}).";
        }
        else
        {
            // Tudo certo: deixa entrar e marca a reserva como usada
            reserva.Estado = EstadoReserva.Usada;
            await _db.SaveChangesAsync();
            vm.Valido = true;
            vm.Mensagem = $"Entrada válida! Pode entrar: {reserva.Quantidade} pessoa(s).";
        }

        return View(vm);
    }
}
