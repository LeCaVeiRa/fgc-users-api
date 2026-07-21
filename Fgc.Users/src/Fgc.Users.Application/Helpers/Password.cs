using System.Security.Cryptography;
using System.Text;


namespace Fgc.Users.Application.Helpers
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool Verify(string password, string passwordHash)
        {
            var hash = Hash(password);
            return hash == passwordHash;
        }
    }
}

