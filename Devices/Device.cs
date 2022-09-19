using Makaretu.Dns;
using System.Net;
using System.Text.Json;

namespace SonoffLanService.Devices;

internal abstract class Device
{
    public string    Name { get; init; }
    public string    Id   { get; init; }
    public string    Key  { get; init; }
    public IPAddress Ip   { get; init; }

    protected Device(string name, string id, string key, IPAddress ip)
    {
        Name = name;
        Id   = id;
        Key  = key;
        Ip   = ip;
    }

    private static readonly Dictionary<string, Device> _all = new()
    {
        {
            "1001666c98", new SwitchBasicR2("Переключатель 1", "1001666c98", "c4d648a1-e77d-4dac-ba9a-fce838fcaaf2", IPAddress.Parse("192.168.1.101"))
        }
    };

    public static Device? Get(string    id)                 => _all.TryGetValue(id, out var v) ? v : null;
    public static T?      Get<T>(string id) where T : class => _all.TryGetValue(id, out var v) ? v as T : null;

    private class Result
    {
        public        int     Error                    { get; set; } = -1;
        public static Result? Deserialize(string data) => JsonSerializer.Deserialize<Result>(data, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }


    protected virtual async Task<bool> Write(string command, string data)
    {
        using var client        = new HttpClient {Timeout = TimeSpan.FromSeconds(3)};
        var       encryptedData = new EncryptData(this, data);

        try
        {
            var response = await client.PostAsync(new Uri($"http://{Ip}:8081/zeroconf/{command}"), encryptedData.GetJsonContent());
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var obj    = Result.Deserialize(result);
            return obj?.Error == 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public abstract void SetData(string data, string iv);

    public static void ServiceInstanceDiscovered(object? sender, ServiceInstanceDiscoveryEventArgs e)
    {
        const string deviceIdPrefix = "eWeLink_";

        var instName = e.ServiceInstanceName.Labels;
        if (instName.Count != 4 ||
            instName[^3] != "_ewelink" ||
            instName[^2] != "_tcp" ||
            instName[^1] != "local" ||
            !instName[0].StartsWith(deviceIdPrefix) ||
            !e.Message.IsResponse)
        {
            return;
        }

        var deviceId = instName[0][deviceIdPrefix.Length..];
        if (!_all.TryGetValue(deviceId, out var device)) return;

        var iv   = string.Empty;
        var data = string.Empty;

        foreach (var answer in e.Message.Answers.Where(x => x.Type == DnsType.TXT).Select(x => x as TXTRecord))
        {
            if (answer?.Strings == null) continue;
            foreach (var answerString in answer.Strings)
            {
                const string ivPrefix   = "iv=";
                const string dataPrefix = "data1=";

                if (answerString.StartsWith(ivPrefix))
                {
                    iv = answerString[ivPrefix.Length..];
                    continue;
                }

                if (answerString.StartsWith(dataPrefix))
                {
                    data = answerString[dataPrefix.Length..];
                }
            }
        }

        device.SetData(data, iv);
    }
}