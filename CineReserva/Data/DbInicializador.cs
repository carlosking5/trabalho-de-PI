using CineReserva.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Data;

// Corre sempre que o site arranca (ver Program.cs).
// Só cria o que ainda não existe, por isso pode correr muitas vezes sem duplicar nada.
public static class DbInicializador
{
    public const string EmailAdmin = "admin@cinema.pt";
    public const string SenhaAdmin = "Admin123!";

    private static readonly string[] CategoriasIniciais =
    {
        "Ação", "Aventura", "Comédia", "Drama", "Romance",
        "Terror", "Ficção Científica", "Crime", "Desporto"
    };

    public static async Task InicializarAsync(IServiceProvider servicos)
    {
        using var scope = servicos.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
        var gestorPerfis = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var gestorUtilizadores = scope.ServiceProvider.GetRequiredService<UserManager<Utilizador>>();

        // 1. Cria o ficheiro cinema.db e todas as tabelas, se ainda não existirem
        await db.Database.EnsureCreatedAsync();

        // 2. Perfis
        foreach (var perfil in new[] { Perfis.Administrador, Perfis.Cliente })
        {
            if (!await gestorPerfis.RoleExistsAsync(perfil))
                await gestorPerfis.CreateAsync(new IdentityRole(perfil));
        }

        // 3. Categorias
        foreach (var nome in CategoriasIniciais)
        {
            if (!await db.Categorias.AnyAsync(c => c.Nome == nome))
                db.Categorias.Add(new Categoria { Nome = nome });
        }
        await db.SaveChangesAsync();

        // 4. Conta de administrador
        if (await gestorUtilizadores.FindByEmailAsync(EmailAdmin) is null)
        {
            var admin = new Utilizador
            {
                UserName = EmailAdmin,
                Email = EmailAdmin,
                EmailConfirmed = true,
                Nome = "Administrador"
            };
            var resultado = await gestorUtilizadores.CreateAsync(admin, SenhaAdmin);
            if (resultado.Succeeded)
                await gestorUtilizadores.AddToRoleAsync(admin, Perfis.Administrador);
        }
    }
}
