using System.Security.Cryptography;
using System.Text;
using ApiBaseCore.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ApiBaseCore.Infrastructure.Services;

/// <summary>
/// Implementation of IEncryptionService using AES-256 (Advanced Encryption Standard).
/// This service is used to encrypt and decrypt sensitive data like passwords (as per requirement).
/// </summary>
public class AESEncryptionService : IEncryptionService
{
    private readonly string _key;
    private readonly string _iv;

    public AESEncryptionService(IConfiguration configuration)
    {
        // In a real scenario, these should be securely stored in Environment Variables or Key Vault.
        // For this exercise, we retrieve them from configuration or use a fallback for dev/testing.
        // AES-256 requires a 32-byte key (256 bits) and 16-byte IV (128 bits).
        
        _key = configuration["Security:EncryptionKey"] ?? "12345678901234567890123456789012"; // 32 chars
        _iv = configuration["Security:EncryptionIV"] ?? "1234567890123456"; // 16 chars
        
        if (_key.Length != 32) throw new ArgumentException("Encryption Key must be 32 characters (256 bits).");
        if (_iv.Length != 16) throw new ArgumentException("Encryption IV must be 16 characters (128 bits).");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try 
        {
            var buffer = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_key);
                aes.IV = Encoding.UTF8.GetBytes(_iv);

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(buffer))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch
        {
            // If decryption fails (e.g. invalid base64 or wrong key), return original text or throw.
            // For robustness in this context, we'll assume if it fails it might not be encrypted or corrupted.
            // But for security, we should probably throw. Let's throw to be safe.
            throw new ArgumentException("Failed to decrypt data.");
        }
    }
}
