using CineReserva.Models;
using CineReserva.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CineReserva.Controllers;

// Entrar, Criar Conta e Sair (usa o ASP.NET Core Identity)
public class ContaController : Controller
{
    private readonly UserManager<Utilizador> _utilizadores;
    private readonly SignInManager<Utilizador> _sessao;

    public ContaController(UserManager<Utilizador> utilizadores, SignInManager<Utilizador> sessao)
    {
        _utilizadores = utilizadores;
        _sessao = sessao;
    }

    [HttpGet]
    public IActionResult Entrar(string? returnUrl) => View(new EntrarViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(EntrarViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // O nome de utilizador é o próprio email
        var resultado = await _sessao.PasswordSignInAsync(vm.Email, vm.Senha, vm.Lembrar, lockoutOnFailure: false);
        if (!resultado.Succeeded)
        {
            ModelState.AddModelError("", "Email ou senha incorretos.");
            return View(vm);
        }

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        // O administrador vai diretamente para o painel
        var utilizador = await _utilizadores.FindByEmailAsync(vm.Email);
        if (utilizador is not null && await _utilizadores.IsInRoleAsync(utilizador, Perfis.Administrador))
            return RedirectToAction("Index", "Painel", new { area = "Admin" });

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult CriarConta(string? returnUrl) => View(new CriarContaViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarConta(CriarContaViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var utilizador = new Utilizador { UserName = vm.Email, Email = vm.Email, Nome = vm.Nome };
        var resultado = await _utilizadores.CreateAsync(utilizador, vm.Senha);

        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro.Description);
            return View(vm);
        }

        // Quem cria conta pelo site é sempre Cliente
        await _utilizadores.AddToRoleAsync(utilizador, Perfis.Cliente);
        await _sessao.SignInAsync(utilizador, isPersistent: false);

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await _sessao.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AcessoNegado() => View();
}
