using Microsoft.AspNetCore.Identity;

namespace CineVerse.Web.Identity;

internal sealed class PortugueseIdentityErrors : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Error(nameof(DefaultError), "Não foi possível concluir a operação.");
    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), "Já existe uma conta com este endereço.");
    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), "Indica um endereço de email válido.");
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"A palavra-passe deve ter pelo menos {length} caracteres.");
    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "Inclui pelo menos um algarismo.");
    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "Inclui pelo menos uma letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "Inclui pelo menos uma letra maiúscula.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "Inclui pelo menos um símbolo.");
    public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch), "A palavra-passe atual está incorreta.");
    public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure), "Houve uma alteração simultânea. Volta a tentar.");

    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };
}
