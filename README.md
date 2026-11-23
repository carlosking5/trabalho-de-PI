# CineVerse

Aplicação de cinema em português de Portugal, desenvolvida em C# com .NET 10 LTS, ASP.NET Core, Blazor interativo no servidor e SQLite.

## Executar no Windows 11

Instala o SDK .NET 10. Na raiz do repositório, executa em PowerShell:

```powershell
dotnet restore CineVerse.sln
dotnet run --project src/CineVerse.Web --launch-profile http
```

Abre [http://localhost:5019](http://localhost:5019). Para terminar, usa Ctrl+C. No Visual Studio, abre `CineVerse.sln` e seleciona `CineVerse.Web` como projeto de arranque. No VS Code, abre a pasta do repositório e executa os mesmos comandos no terminal integrado.

O arranque aplica as migrações pendentes e, na primeira execução, cria `src/CineVerse.Web/cineverse.db` com seis títulos fictícios. A migração PlatformSchema acrescenta géneros, temporadas, episódios, um cinema com 40 lugares e três planos de subscrição. Podes abrir o ficheiro no DB Browser for SQLite. A configuração `ConnectionStrings__CineVerse` permite indicar outra cadeia de ligação.

## Usar o dashboard

Seleciona Filmes, Séries ou Animes no menu. A pesquisa ignora maiúsculas e acentos. O botão de filtros permite escolher género, ano e ordenação. Seleciona um cartão para abrir os detalhes ou adiciona títulos aos favoritos durante a visita. No telemóvel, abre o menu pelo botão junto ao logótipo.

## Verificar e gerir migrações

```powershell
dotnet build CineVerse.sln
dotnet test CineVerse.sln
dotnet tool restore
dotnet ef migrations list --project src/CineVerse.Infrastructure --startup-project src/CineVerse.Web
```

Os testes xUnit usam SQLite em memória e verificam a criação do catálogo, a atualização da base de dados da Fase 1 e a rejeição de duas atribuições do mesmo lugar na mesma sessão.

## Estrutura e estado

- `CineVerse.Domain` contém os modelos de catálogo, reservas, perfis e subscrições.
- `CineVerse.Application` define o contrato de consulta.
- `CineVerse.Infrastructure` contém o contexto EF Core, as configurações do esquema, as migrações e o serviço de catálogo.
- `CineVerse.Web` contém o dashboard, os recursos locais e a configuração da aplicação.
- `CineVerse.Tests` contém o teste de integração do catálogo.

A Fase 2 acrescenta as tabelas do ASP.NET Core Identity, géneros, temporadas, episódios, histórico de reprodução, reservas, bilhetes, subscrições e auditoria. O dashboard continua a usar o catálogo demonstrativo e os favoritos duram enquanto a ligação Blazor se mantém. A Fase 3 liga o registo e o início de sessão às tabelas Identity e protege as páginas com papéis. A reprodução, os serviços de reserva e bilhética, as subscrições e a administração são etapas posteriores.

As seis imagens locais foram criadas com a ferramenta integrada ImageGen a partir de uma folha de contacto com cenas originais: astronauta junto a um eclipse, detetive numa Lisboa chuvosa, expedição na neve, duas mulheres num metro noturno, adolescente numa paisagem rural animada e piloto numa cabine espacial.
