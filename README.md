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

O painel de episódios recentes apresenta as últimas publicações da SQLite. Seleciona um episódio para consultar o título e a numeração da temporada. O navegador guarda a preferência de tema claro ou escuro.

## Registar e gerir o acesso

No dashboard, seleciona **Iniciar sessão** e depois **Cria uma conta**. O registo público atribui o papel **Utilizador**. A página **A minha conta** apresenta o email e permite terminar a sessão. Cinco tentativas falhadas bloqueiam a entrada na conta durante 15 minutos.

Para criar uma conta **Administrador**, executa o comando abaixo no terminal local do projeto. Usa um email ainda por registar. A opção `-MaskInput` está disponível no PowerShell 7.2 ou posterior.

```powershell
$env:CINEVERSE_ADMIN_EMAIL = Read-Host 'Email do administrador'
$env:CINEVERSE_ADMIN_PASSWORD = Read-Host 'Palavra-passe do administrador' -MaskInput
try {
    dotnet run --project src/CineVerse.Web --launch-profile http -- --criar-admin
} finally {
    Remove-Item Env:CINEVERSE_ADMIN_EMAIL, Env:CINEVERSE_ADMIN_PASSWORD
}
```

Se o servidor já estiver a correr no mesmo terminal, termina-o antes com Ctrl+C. Depois de criar a conta, arranca a aplicação normalmente, inicia sessão e abre **Área de administração**. A página verifica o papel no servidor. Em produção, serve a aplicação por HTTPS para que o navegador envie o cookie de sessão.

## Verificar e gerir migrações

```powershell
dotnet build CineVerse.sln
dotnet test CineVerse.sln
dotnet tool restore
dotnet ef migrations list --project src/CineVerse.Infrastructure --startup-project src/CineVerse.Web
```

Os testes xUnit verificam as migrações e a restrição de lugares em SQLite em memória. Os testes HTTP usam bases SQLite temporárias e verificam registo, login, logout, proteção antifalsificação, bloqueio por tentativas falhadas e acesso por papel.

## Estrutura e estado

- `CineVerse.Domain` contém os modelos de catálogo, reservas, perfis e subscrições.
- `CineVerse.Application` define o contrato de consulta.
- `CineVerse.Infrastructure` contém o contexto EF Core, as configurações do esquema, as migrações e o serviço de catálogo.
- `CineVerse.Web` contém o dashboard, os recursos locais e a configuração da aplicação.
- `CineVerse.Tests` contém o teste de integração do catálogo.

A Fase 4 consolida o dashboard responsivo com secções por tipo de conteúdo, episódios recentes e páginas de ajuda e apresentação. A confirmação de email, a recuperação de palavra-passe, a edição de perfil, os favoritos persistentes, a reprodução, as reservas, as subscrições e as ferramentas de administração ainda estão pendentes.

As seis imagens locais foram criadas com a ferramenta integrada ImageGen a partir de uma folha de contacto com cenas originais: astronauta junto a um eclipse, detetive numa Lisboa chuvosa, expedição na neve, duas mulheres num metro noturno, adolescente numa paisagem rural animada e piloto numa cabine espacial.
 vida bosa