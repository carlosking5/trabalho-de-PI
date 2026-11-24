using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using CineReserva.Models;

namespace CineReserva.Infraestrutura;

// Pequenas funções de ajuda usadas nas páginas
public static class Extensoes
{
    private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-PT");

    // Devolve o nome "bonito" de um enum: TipoConteudo.Serie -> "Série"
    public static string Nome(this Enum valor)
    {
        var membro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        return membro?.GetCustomAttribute<DisplayAttribute>()?.Name ?? valor.ToString();
    }

    // 7.5 -> "7,50 €"
    public static string Euros(this decimal valor) => valor.ToString("0.00", Pt) + " €";

    // "seg., 24/11 · 21:30"
    public static string DiaHora(this DateTime d) => d.ToString("ddd, dd/MM · HH:mm", Pt);

    // "24/11/2026"
    public static string Dia(this DateTime d) => d.ToString("dd/MM/yyyy", Pt);

    // 135 -> "2h 15min"
    public static string Duracao(this int minutos) =>
        minutos >= 60 ? $"{minutos / 60}h {minutos % 60:00}min" : $"{minutos}min";

    // Cor do badge de cada estado de reserva
    public static string Cor(this EstadoReserva estado) => estado switch
    {
        EstadoReserva.Ativa => "success",
        EstadoReserva.Usada => "secondary",
        _ => "danger"
    };
}
