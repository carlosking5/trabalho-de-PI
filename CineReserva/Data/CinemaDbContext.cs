using CineReserva.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineReserva.Data;

// IdentityDbContext já traz as tabelas do login (AspNetUsers, AspNetRoles, ...).
// Nós acrescentamos as tabelas do cinema.
public class CinemaDbContext : IdentityDbContext<Utilizador>
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Filme> Filmes => Set<Filme>();
    public DbSet<Sessao> Sessoes => Set<Sessao>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // obrigatório por causa do Identity

        // Não deixa haver duas categorias com o mesmo nome
        builder.Entity<Categoria>().HasIndex(c => c.Nome).IsUnique();

        // Não se pode apagar uma categoria que ainda tenha filmes
        builder.Entity<Filme>()
            .HasOne(f => f.Categoria)
            .WithMany(c => c.Filmes)
            .HasForeignKey(f => f.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Apagar um filme apaga as suas sessões
        builder.Entity<Sessao>()
            .HasOne(s => s.Filme)
            .WithMany(f => f.Sessoes)
            .HasForeignKey(s => s.FilmeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Sessao>().Property(s => s.Preco).HasPrecision(8, 2);

        // Apagar uma sessão apaga as reservas dessa sessão
        builder.Entity<Reserva>()
            .HasOne(r => r.Sessao)
            .WithMany(s => s.Reservas)
            .HasForeignKey(r => r.SessaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Reserva>()
            .HasOne(r => r.Utilizador)
            .WithMany(u => u.Reservas)
            .HasForeignKey(r => r.UtilizadorId)
            .OnDelete(DeleteBehavior.Cascade);

        // O código da reserva é único (é o que se valida à entrada)
        builder.Entity<Reserva>().HasIndex(r => r.Codigo).IsUnique();
        builder.Entity<Reserva>().Property(r => r.Codigo).HasMaxLength(8);
    }
}
