using System.Security.Cryptography;

namespace MediTrack.Api.Common
{
    public static class Utils
    {
        public static byte[] GenerateRandomRowVersion()
        {
            byte[] randomBytes = new byte[8]; // exactly 8 bytes

            // Secure and modern option (Recommended for .NET Core/6/8 environments)
            RandomNumberGenerator.Fill(randomBytes);

            return randomBytes;
        }
    }
}