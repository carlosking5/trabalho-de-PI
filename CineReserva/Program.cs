using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using CineReserva.Data;
using CineReserva.Infraestrutura;
using CineReserva.Models;
using CineReserva.Servicos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. SERVIÇOS ----------

// Base de dados SQLite (o ficheiro cinema.db fica na pasta do projeto)
builder.Services.AddDbContext<CinemaDbContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("Cinema")));

// Identity: contas, senhas e perfis (Administrador / Cliente)
builder.Services
    .AddIdentity<Utilizador, IdentityRole>(opcoes =>
    {
        opcoes.User.RequireUniqueEmail = true;
        opcoes.Password.RequiredLength = 6;
        opcoes.Password.RequireDigit = true;
        opcoes.Password.RequireLowercase = true;
        opcoes.Password.RequireUppercase = true;
        opcoes.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<CinemaDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<ErrosIdentityPortugues>();

// Para onde vai o utilizador quando precisa de fazer login ou não tem permissão
builder.Services.ConfigureApplicationCookie(opcoes =>
{
    opcoes.LoginPath = "/Conta/Entrar";
    opcoes.AccessDeniedPath = "/Conta/AcessoNegado";
});

// MVC (Controllers + Views), com o nosso leitor de números decimais
builder.Services.AddControllersWithViews(opcoes =>
{
    opcoes.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
    opcoes.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Este campo é obrigatório.");
    opcoes.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((valor, _) => $"O valor '{valor}' não é válido.");
    opcoes.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(_ => "Tem de ser um número.");
});

// Escreve os acentos (ç, ã, é...) diretamente no HTML em vez de códigos como &#xE7;
builder.Services.Configure<WebEncoderOptions>(opcoes =>
    opcoes.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// O nosso serviço que grava os cartazes em wwwroot/imagens
builder.Services.AddSingleton<GestorImagens>();

var app = builder.Build();

// ---------- 2. PIPELINE (ordem pela qual cada pedido é tratado) ----------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();
}

// Site em português de Portugal: datas dd/MM/yyyy, nomes dos dias, etc.
var pt = new CultureInfo("pt-PT");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(pt),
    SupportedCultures = new[] { pt },
    SupportedUICultures = new[] { pt }
});

app.UseHttpsRedirection();
app.UseStaticFiles();      // serve os ficheiros de wwwroot (css, js, imagens)
app.UseRouting();
app.UseAuthentication();   // quem é o utilizador?
app.UseAuthorization();    // o que é que ele pode fazer?

// Rota da área de administração: /Admin/Filmes, /Admin/Sessoes, ...
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Painel}/{action=Index}/{id?}");

// Rota normal do site: /Filmes/Detalhes/3, ...
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------- 3. DADOS INICIAIS ----------
await DbInicializador.InicializarAsync(app.Services);

app.Run();
