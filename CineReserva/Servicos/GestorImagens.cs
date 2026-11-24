namespace CineReserva.Servicos;

// Grava e apaga os cartazes na pasta wwwroot/imagens
public class GestorImagens
{
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long TamanhoMaximo = 5 * 1024 * 1024; // 5 MB

    private readonly string _pasta;

    public GestorImagens(IWebHostEnvironment ambiente)
    {
        _pasta = Path.Combine(ambiente.WebRootPath, "imagens");
        Directory.CreateDirectory(_pasta);
    }

    // Devolve uma mensagem de erro, ou null se o ficheiro for válido
    public string? Validar(IFormFile ficheiro)
    {
        var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(extensao))
            return "O cartaz tem de ser uma imagem (.jpg, .png, .webp ou .gif).";
        if (ficheiro.Length > TamanhoMaximo)
            return "A imagem não pode ter mais de 5 MB.";
        return null;
    }

    // Grava a imagem com um nome único e devolve o caminho para usar no site
    public async Task<string> GuardarAsync(IFormFile ficheiro)
    {
        var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
        var nome = $"{Guid.NewGuid():N}{extensao}";

        await using var destino = File.Create(Path.Combine(_pasta, nome));
        await ficheiro.CopyToAsync(destino);

        return $"/imagens/{nome}";
    }

    // Apaga uma imagem antiga (quando se troca o cartaz ou se apaga o filme)
    public void Apagar(string? caminho)
    {
        if (string.IsNullOrEmpty(caminho) || !caminho.StartsWith("/imagens/")) return;

        var nome = Path.GetFileName(caminho);
        if (nome == "sem-cartaz.svg") return;

        var completo = Path.Combine(_pasta, nome);
        if (File.Exists(completo)) File.Delete(completo);
    }
}
