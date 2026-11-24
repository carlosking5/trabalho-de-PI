using CineReserva.Models;

namespace CineReserva.ViewModels;

// Um "cartão" da grelha de cartazes: o filme + a informação da próxima sessão.
// O Filme tem de vir com as Sessoes e as Reservas carregadas.
public class FilmeCartao
{
    // Filmes com sessão nos próximos 7 dias estão "Em cartaz"; os outros aparecem em "Brevemente"
    public const int DiasEmCartaz = 7;

    public Filme Filme { get; }
    public List<Sessao> SessoesFuturas { get; }

    public FilmeCartao(Filme filme)
    {
        Filme = filme;
        SessoesFuturas = filme.Sessoes
            .Where(s => !s.JaPassou)
            .OrderBy(s => s.DataHora)
            .ToList();
    }

    public Sessao? ProximaSessao => SessoesFuturas.FirstOrDefault();
    public Sessao? ProximaDisponivel => SessoesFuturas.FirstOrDefault(s => !s.Esgotada);

    // Tem sessões futuras mas estão todas esgotadas
    public bool Esgotado => SessoesFuturas.Count > 0 && ProximaDisponivel is null;

    public bool EmCartaz => ProximaSessao is not null &&
                            ProximaSessao.DataHora <= DateTime.Now.AddDays(DiasEmCartaz);

    // Sessões só daqui a mais de 7 dias, ou filme acabado de adicionar ainda sem sessões
    public bool Brevemente => !EmCartaz && (ProximaSessao is not null || Filme.Sessoes.Count == 0);

    // Filmes cujas sessões já passaram todas não aparecem ao cliente
    public bool Visivel => EmCartaz || Brevemente;

    public decimal? Preco => (ProximaDisponivel ?? ProximaSessao)?.Preco;
}

// Página inicial
public class InicioViewModel
{
    public FilmeCartao? Destaque { get; set; }
    public List<FilmeCartao> EmCartaz { get; set; } = new();
    public List<FilmeCartao> Brevemente { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
}

// Página "Filmes" com filtros
public class CatalogoViewModel
{
    public List<FilmeCartao> Filmes { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();

    // Filtros escolhidos
    public string? Pesquisa { get; set; }
    public int? CategoriaId { get; set; }
    public TipoConteudo? Tipo { get; set; }
}
