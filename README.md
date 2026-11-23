# CineVerse

Aplicação de cinema em português de Portugal, desenvolvida em C# com .NET 10 LTS, ASP.NET Core, Blazor interativo no servidor e SQLite.

## Executar no Windows 11

Instala o SDK .NET 10. Na raiz do repositório, executa em PowerShell:

```powershell
dotnet restore CineVerse.sln
dotnet run --project src/CineVerse.Web --launch-profile http
```

Abre [http://localhost:5019](http://localhost:5019). Para terminar, usa Ctrl+C. No Visual Studio, abre `CineVerse.sln` e seleciona `CineVerse.Web` como projeto de arranque. No VS Code, abre a pasta do repositório e executa os mesmos comandos no terminal integrado.

O primeiro arranque aplica a migração inicial e cria `src/CineVerse.Web/cineverse.db` com seis títulos fictícios. Podes abrir o ficheiro no DB Browser for SQLite. A configuração `ConnectionStrings__CineVerse` permite indicar outra cadeia de ligação.

## Usar o dashboard

Seleciona Filmes, Séries ou Animes no menu. A pesquisa ignora maiúsculas e acentos. O botão de filtros permite escolher género, ano e ordenação. Seleciona um cartão para abrir os detalhes ou adiciona títulos aos favoritos durante a visita. No telemóvel, abre o menu pelo botão junto ao logótipo.

## Verificar e gerir migrações

```powershell
dotnet build CineVerse.sln
dotnet test CineVerse.sln
dotnet tool restore
dotnet ef migrations list --project src/CineVerse.Infrastructure --startup-project src/CineVerse.Web
```

O teste xUnit aplica a migração a uma SQLite em memória e verifica a leitura e a ordenação dos títulos através do serviço de catálogo.

## Estrutura e estado

- `CineVerse.Domain` contém o modelo inicial do catálogo.
- `CineVerse.Application` define o contrato de consulta.
- `CineVerse.Infrastructure` contém o contexto EF Core, a migração e o serviço SQLite.
- `CineVerse.Web` contém o dashboard, os recursos locais e a configuração da aplicação.
- `CineVerse.Tests` contém o teste de integração do catálogo.

Esta entrega cobre a configuração inicial e a primeira versão do dashboard. O catálogo é demonstrativo e os favoritos duram enquanto a ligação Blazor se mantém. A modelação completa de temporadas e episódios, as contas e papéis, o player e o progresso, as reservas e bilhetes, as subscrições e a administração fazem parte das etapas seguintes.

As seis imagens locais foram criadas com a ferramenta integrada ImageGen a partir de uma folha de contacto com cenas originais: astronauta junto a um eclipse, detetive numa Lisboa chuvosa, expedição na neve, duas mulheres num metro noturno, adolescente numa paisagem rural animada e piloto numa cabine espacial.
