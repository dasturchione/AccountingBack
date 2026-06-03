namespace Application.Abstractions.Authentication
{
    public interface IPasswordHasher
    {
        string GenerateSalt();

        string Hash(string password, string passwordSalt);

        bool Verify(string password, string passwordSalt, string passwordHash);
    }
}
