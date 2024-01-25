using System.Security.Cryptography;

namespace BlogApp;

public static class Configuration
{
    private static byte[] key = Generate256BitsOfRandomEntropy();
    public static string JwtKey { get; set; } = Convert.ToBase64String(key);
    public static SmtpConfiguration Smtp = new();

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        
        byte[] randomBytes = new byte[32];
        rngCryptoServiceProvider.GetBytes(randomBytes);
        return randomBytes;
    }

    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}