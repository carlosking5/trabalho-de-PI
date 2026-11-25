using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CineReserva.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineReserva.ViewModels;

// Ecrã de pagamento (simulação). O cliente escolhe Cartão ou MB WAY e preenche os dados.
// IMPORTANTE: estes dados só servem para validar o formulário. O controller grava apenas
// o método escolhido; o número do cartão, o CVV e o telemóvel nunca vão para a base de dados.
public class PagamentoViewModel : IValidatableObject
{
    public int ReservaId { get; set; }

    [Display(Name = "Método de pagamento")]
    [Required(ErrorMessage = "Escolha Cartão ou MB WAY.")]
    public MetodoPagamento? Metodo { get; set; }

    // ----- MB WAY -----
    [Display(Name = "Número de telemóvel")]
    public string? Telemovel { get; set; }

    // ----- Cartão -----
    [Display(Name = "Nome no cartão")]
    public string? NomeCartao { get; set; }

    [Display(Name = "Número do cartão")]
    public string? NumeroCartao { get; set; }

    [Display(Name = "Validade (MM/AA)")]
    public string? Validade { get; set; }

    [Display(Name = "CVV")]
    public string? Cvv { get; set; }

    // Só para mostrar o resumo (não vem do formulário)
    [BindNever, ValidateNever]
    public Reserva? Reserva { get; set; }

    // Tira espaços e hífenes: "912 345 678" -> "912345678", "4111-1111-..." -> "4111111..."
    public static string SoDigitos(string? texto) => Regex.Replace(texto ?? "", @"[\s-]", "");

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Metodo == MetodoPagamento.MbWay)
        {
            // Telemóvel português: 9 dígitos começados por 9
            if (!Regex.IsMatch(SoDigitos(Telemovel), @"^9\d{8}$"))
                yield return Erro(nameof(Telemovel), "Indique um telemóvel português com 9 dígitos começado por 9 (ex: 912 345 678).");
        }
        else if (Metodo == MetodoPagamento.Cartao)
        {
            var nome = (NomeCartao ?? "").Trim();
            if (nome.Length == 0)
                yield return Erro(nameof(NomeCartao), "Indique o nome que aparece no cartão.");
            else if (nome.Length > 100 || !Regex.IsMatch(nome, @"^[\p{L}][\p{L} '.-]*$"))
                yield return Erro(nameof(NomeCartao), "O nome só pode ter letras e espaços (máximo 100).");

            if (!Regex.IsMatch(SoDigitos(NumeroCartao), @"^\d{16}$"))
                yield return Erro(nameof(NumeroCartao), "O número do cartão tem de ter 16 dígitos.");

            var erroValidade = ErroValidade(Validade, DateTime.Today);
            if (erroValidade is not null)
                yield return Erro(nameof(Validade), erroValidade);

            if (!Regex.IsMatch((Cvv ?? "").Trim(), @"^\d{3}$"))
                yield return Erro(nameof(Cvv), "O CVV tem de ter 3 dígitos.");
        }
    }

    // "MM/AA". O cartão é válido até ao último dia do mês indicado.
    public static string? ErroValidade(string? validade, DateTime hoje)
    {
        var m = Regex.Match((validade ?? "").Trim(), @"^(\d{2})\s*/\s*(\d{2})$");
        if (!m.Success)
            return "Escreva a validade no formato MM/AA (ex: 08/28).";

        var mes = int.Parse(m.Groups[1].Value);
        var ano = 2000 + int.Parse(m.Groups[2].Value);
        if (mes < 1 || mes > 12)
            return "O mês da validade tem de estar entre 01 e 12.";

        if (ano < hoje.Year || (ano == hoje.Year && mes < hoje.Month))
            return "Este cartão já expirou.";

        return null;
    }

    private static ValidationResult Erro(string campo, string mensagem) => new(mensagem, new[] { campo });
}
