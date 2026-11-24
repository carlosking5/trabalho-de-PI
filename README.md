# CineReserva — Site de reserva de bilhetes de cinema

Projeto escolar em **ASP.NET Core MVC (C#)** com **Entity Framework Core + SQLite** e **ASP.NET Core Identity**.

## Como correr

1. Instalar o **.NET 10 SDK** (https://dotnet.microsoft.com/download).
2. Abrir um terminal na pasta `CineReserva` e correr:
   ```
   dotnet run
   ```
   (ou abrir `CineReserva.slnx` no Visual Studio e carregar em ▶).
3. Abrir o endereço que aparece no terminal (ex: `https://localhost:7180`).

Na primeira execução é criado automaticamente o ficheiro `cinema.db` com as tabelas,
as 9 categorias e a conta de administrador.
Para recomeçar com a base de dados vazia, basta apagar o `cinema.db`.

| Conta inicial | Email | Senha |
|---|---|---|
| Administrador | `admin@cinema.pt` | `Admin123!` |

Os clientes criam a sua conta em **Criar Conta**.

## Estrutura (padrão MVC)

```
CineReserva/
├── Program.cs                 Arranque: base de dados, Identity, rotas, cultura pt-PT
├── Models/                    As tabelas: Categoria, Filme, Sessao, Reserva, Utilizador
├── Data/
│   ├── CinemaDbContext.cs     Liga os modelos ao SQLite e define as relações
│   ├── DbInicializador.cs     Cria categorias, perfis e o administrador ao arrancar
│   └── ErrosIdentityPortugues.cs  Mensagens de erro do login em português
├── ViewModels/                Dados de cada formulário/página (ex: FilmeFormViewModel)
├── Servicos/                  GestorImagens (grava cartazes) e GeradorCodigo (códigos de reserva)
├── Infraestrutura/            Ajudas: formatar euros/datas, aceitar "7,50" e "7.50"
├── Controllers/               Área do cliente: Home, Filmes, Reservas, Conta
├── Views/                     Páginas do cliente (.cshtml)
├── Areas/Admin/               Área do administrador (só perfil Administrador)
│   ├── Controllers/           Painel, Categorias, Filmes, Sessoes, Reservas, Validar, Relatorios
│   └── Views/
└── wwwroot/                   Ficheiros públicos: css, js e imagens (cartazes)
```

## Regras de negócio e onde estão no código

| Regra | Onde |
|---|---|
| O cliente não escolhe cadeira | `Reserva` só tem `Quantidade` (não há tabela de lugares) |
| Nº de bilhetes = nº de lugares da sala | `Sessao.Lotacao`; `Sessao.LugaresRestantes = Lotacao − BilhetesVendidos` |
| "Esgotado" e não deixa reservar | `Sessao.Esgotada`; verificado em `ReservasController.VerificarSessao` |
| Só sessões futuras | `Sessao.JaPassou`; verificado ao abrir e ao confirmar a reserva |
| Nunca vender bilhetes a mais | A reserva é gravada dentro de uma transação (`ReservasController.Criar`) |
| Só o administrador gere filmes/sessões | `[Authorize(Roles = "Administrador")]` em todos os controllers de `Areas/Admin` |
| Reservas canceladas libertam bilhetes | `BilhetesVendidos` ignora reservas com estado `Cancelada` |
| Validar entrada | `ValidarController`: código tem de existir, estar `Ativa` e ser para hoje → passa a `Usada` |

## "Em cartaz" e "Brevemente"

- **Em cartaz**: filmes com uma sessão nos próximos 7 dias.
- **Brevemente**: filmes cujas sessões são só daqui a mais de 7 dias, ou que ainda não têm sessões.
- Filmes cujas sessões já passaram todas deixam de aparecer ao cliente.

## Notas técnicas

- A base de dados é criada com `EnsureCreated()` (simples, sem migrações). Se alterar os modelos, apague o `cinema.db` para ser recriado.
- O Bootstrap e os ícones são carregados por CDN, por isso é preciso ligação à internet para o visual aparecer.
- Os cartazes carregados ficam em `wwwroot/imagens/` com um nome único (GUID).
