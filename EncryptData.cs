using SonoffLanService.Devices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SonoffLanService;

internal class EncryptData
{
    public string Sequence   => (DateTime.Now.Ticks / 1000000).ToString();
    public string Deviceid   { get; init; }
    public string SelfApikey => "458826d3-72e1-443c-a798-4c32d51bc5de";
    public string Iv         { get; init; }
    public bool   Encrypt    => true;
    public string Data       { get; init; }

    public EncryptData(Device device, string data)
    {
        Deviceid = device.Id;

        var hash = MD5.Create();
        var key  = hash.ComputeHash(Encoding.UTF8.GetBytes(device.Key));

        using var aesAlg = Aes.Create();
        aesAlg.Mode    = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;

        aesAlg.Key = key;

        var encryptor = aesAlg.CreateEncryptor();

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(data);
        }

        Data = Convert.ToBase64String(msEncrypt.ToArray());
        Iv   = Convert.ToBase64String(aesAlg.IV);
    }

    public          StringContent GetJsonContent() => new(ToString(), Encoding.UTF8, "application/json");
    public override string        ToString()       => JsonSerializer.Serialize(this, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
}