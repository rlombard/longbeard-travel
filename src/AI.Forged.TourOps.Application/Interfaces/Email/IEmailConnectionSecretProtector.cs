namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailConnectionSecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}
