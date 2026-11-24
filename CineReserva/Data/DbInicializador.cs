using CineReserva.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

        // 1. Cria/atualiza a base de dados aplicando as migrations que faltam (pasta Data/Migrations)
        await RegistarBaseAntigaAsync(db);
        await db.Database.MigrateAsync();

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

    // A primeira versão criava a base com EnsureCreated(), que não guarda o histórico de migrations.
    // Se encontrarmos uma base dessas (tem a tabela Filmes mas não tem __EFMigrationsHistory),
    // registamos a migration "Inicial" como já aplicada. Assim o Migrate() só aplica as novas
    // e ninguém perde os dados.
    private static async Task RegistarBaseAntigaAsync(CinemaDbContext db)
    {
        if (!File.Exists(db.Database.GetDbConnection().DataSource)) return; // base nova: o Migrate cria tudo

        var temFilmes = await TabelaExisteAsync(db, "Filmes");
        var temHistorico = await TabelaExisteAsync(db, "__EFMigrationsHistory");
        if (!temFilmes || temHistorico) return;

        var inicial = db.Database.GetMigrations().First(); // ex: "20261124201837_Inicial"
        await db.Database.ExecuteSqlRawAsync(
            "CREATE TABLE \"__EFMigrationsHistory\" (" +
            "\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, " +
            "\"ProductVersion\" TEXT NOT NULL)");
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"__EFMigrationsHistory\" VALUES ({0}, {1})",
            inicial, ProductInfo.GetVersion());
    }

    private static async Task<bool> TabelaExisteAsync(CinemaDbContext db, string nome)
    {
        var total = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name = {0}", nome)
            .SingleAsync();
        return total > 0;
    }
}
