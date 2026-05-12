using System.Security.Cryptography;
using System.Text;

namespace Backend.Features.Auth.Application
{
    public static class CryptoUtil
    {
        public static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public static string CreateOpaqueToken(int byteLength = 48)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
