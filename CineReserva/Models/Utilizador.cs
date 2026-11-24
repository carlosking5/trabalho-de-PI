using Microsoft.AspNetCore.Identity;

namespace CineReserva.Models;

// Conta de utilizador. Herda tudo do Identity (Email, PasswordHash, ...)
// e acrescentamos o nome para mostrar no site.
public class Utilizador : IdentityUser
{
    public string Nome { get; set; } = "";

    public List<Reserva> Reservas { get; set; } = new();
}
