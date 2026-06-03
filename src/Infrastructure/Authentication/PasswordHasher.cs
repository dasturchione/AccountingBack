using Application.Abstractions.Authentication;
using System.Security.Cryptography;

namespace Infrastructure.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 500_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string GenerateSalt()
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            return Convert.ToBase64String(saltBytes);
        }

        public string Hash(string password, string passwordSalt)
        {
            var saltBytes = Convert.FromBase64String(passwordSalt);

            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, Algorithm, KeySize);

            return Convert.ToBase64String(hashBytes);
        }

        public bool Verify(string password, string passwordSalt, string passwordHash)
        {
            var computedHash = Hash(password, passwordSalt);

            return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(passwordHash), Convert.FromBase64String(computedHash));
        }
    }
}
