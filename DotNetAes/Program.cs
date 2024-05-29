namespace DotNetAes;

using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static byte[] _KEY = Encoding.UTF8.GetBytes("TbYYPFKe3C26uARYM0Wp/Fc8kzWFeMq0");
    private static byte[] _IV = Encoding.UTF8.GetBytes("MLlkjgO7Xar6xniK");

    static async Task Main(string[] args)
    {
        string data = $"This is the content";
        Console.WriteLine(data);

        var encrypted = await AESEncryptAsync(data);
        Console.WriteLine(Encoding.UTF8.GetString(encrypted));

        string decryptStr = await AESDecryptAsync(encrypted);
        Console.WriteLine(decryptStr);

        Console.ReadLine();
    }

    private static async Task<byte[]> AESEncryptAsync(string plainText)
    {
        using var aes = Aes.Create();
        using var memoryStream = new MemoryStream();
        using var encryptor = new CryptoStream(memoryStream, aes.CreateEncryptor(_KEY, _IV), CryptoStreamMode.Write);
        using (var streamWriter = new StreamWriter(encryptor))
            await streamWriter.WriteAsync(plainText);

        return memoryStream.ToArray();
    }

    public static async Task<string> AESDecryptAsync(byte[] cipherText)
    {
        using var aes = Aes.Create();
        using var memoryStream = new MemoryStream(cipherText);
        using var decryptor = new CryptoStream(memoryStream, aes.CreateDecryptor(_KEY, _IV), CryptoStreamMode.Read);
        using var streamReader = new StreamReader(decryptor);

        return await streamReader.ReadToEndAsync();
    }
}