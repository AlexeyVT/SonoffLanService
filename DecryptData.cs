using System.Security.Cryptography;
using System.Text;

namespace SonoffLanService;

internal static class DecryptData
{
    public static string ReadAsString(string data, string iv, string deviceKey)
    {
        using var aesAlg = Aes.Create();
        aesAlg.IV      = Convert.FromBase64String(iv);
        aesAlg.Mode    = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.Key     = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(deviceKey));

        var decryptor = aesAlg.CreateDecryptor();

        using var ms = new MemoryStream();
        ms.Write(Convert.FromBase64String(data));
        ms.Position = 0;
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}