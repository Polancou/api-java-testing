namespace ApiBaseCore.Application.Interfaces;

/// <summary>
/// Interface for encryption services.
/// Provides methods for encrypting and decrypting data.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts the specified plain text using AES256.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>The encrypted text in Base64 format.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts the specified cipher text using AES256.
    /// </summary>
    /// <param name="cipherText">The encrypted text (Base64) to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    string Decrypt(string cipherText);
}
