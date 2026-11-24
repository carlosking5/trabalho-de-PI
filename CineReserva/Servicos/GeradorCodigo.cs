using System.Security.Cryptography;

namespace CineReserva.Servicos;

// Gera códigos de reserva como "K7P2QX9M".
// Não usa O/0 nem I/1 para não haver confusões ao escrever o código à entrada.
public static class GeradorCodigo
{
    private const string Caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Novo(int tamanho = 8)
    {
        var letras = new char[tamanho];
        for (int i = 0; i < tamanho; i++)
            letras[i] = Caracteres[RandomNumberGenerator.GetInt32(Caracteres.Length)];
        return new string(letras);
    }
}
