using System.ComponentModel.DataAnnotations;
using CineReserva.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineReserva.ViewModels;

// Página de reserva: resumo da sessão + quantidade de bilhetes
public class CriarReservaViewModel
{
    public int SessaoId { get; set; }

    [Range(1, 1000, ErrorMessage = "Escolha pelo menos 1 bilhete.")]
    public int Quantidade { get; set; } = 1;

    // Só para mostrar (não vem do formulário, por isso não é lido nem validado)
    [BindNever, ValidateNever]
    public Sessao? Sessao { get; set; }
}
