using System.Security.Cryptography;
using System.Text;

namespace ClipboardApi.Services;

public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string key)
    {
        _key = Encoding.UTF8.GetBytes(key);
    }

    public string Encrypt(string decrypted)
    {
        byte[] encryptedBytes;

        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            aes.GenerateIV();

            var plainBytes = Encoding.UTF8.GetBytes(decrypted);
            using (var encryptor = aes.CreateEncryptor())
            {
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    encryptedBytes = memoryStream.ToArray();
                }
            }
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string encrypted)
    {
        var encryptedBytes = Convert.FromBase64String(encrypted);
        byte[] decryptedBytes;

        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            var iv = new byte[aes.IV.Length];
            Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor())
            {
                using (var memoryStream = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    decryptedBytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                }
            }
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}