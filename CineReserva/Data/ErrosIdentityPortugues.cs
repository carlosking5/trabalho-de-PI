using Microsoft.AspNetCore.Identity;

namespace CineReserva.Data;

// Por defeito o Identity mostra os erros em inglês ("Passwords must have...").
// Esta classe traduz as mensagens que aparecem ao criar conta.
public class ErrosIdentityPortugues : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"Já existe uma conta com o email {email}." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"Já existe uma conta com o email {userName}." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "O email não é válido." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = "O email não é válido." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"A senha tem de ter pelo menos {length} caracteres." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "A senha tem de ter pelo menos um número." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "A senha tem de ter pelo menos uma letra minúscula." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "A senha tem de ter pelo menos uma letra maiúscula." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "A senha tem de ter pelo menos um símbolo (ex: ! ? #)." };

    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Ocorreu um erro. Tente novamente." };
}
