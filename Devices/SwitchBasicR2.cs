using System.Net;
using System.Text.Json;

namespace SonoffLanService.Devices;

internal class SwitchBasicR2 : Device
{
    public bool Switch     { get; private set; }
    public bool Startup    { get; private set; }
    public bool Pulse      { get; private set; }
    public bool SledOnline { get; private set; }
    public int  PulseWidth { get; private set; }

    public SwitchBasicR2(string name, string id, string key, IPAddress ip) : base(name, id, key, ip) { }

    public Task Toggle()    => Switch ? SwitchOff() : SwitchOn();
    public Task SwitchOn()  => base.Write("switch", "{\"switch\":\"on\"}");
    public Task SwitchOff() => base.Write("switch", "{\"switch\":\"off\"}");

    private record DataModel(string Switch,
                             string Startup,
                             string Pulse,
                             string SledOnline,
                             int    PulseWidth);

    public override void SetData(string data, string iv)
    {
        var stringData = DecryptData.ReadAsString(data, iv, Key);

        var objData = JsonSerializer.Deserialize<DataModel>(stringData, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        ArgumentNullException.ThrowIfNull(objData);

        Switch     = objData.Switch == "on";
        Startup    = objData.Startup == "on";
        Pulse      = objData.Pulse == "on";
        SledOnline = objData.SledOnline == "on";
        PulseWidth = objData.PulseWidth;
    }
}