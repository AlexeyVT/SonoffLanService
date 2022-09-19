// See https://aka.ms/new-console-template for more information

using Makaretu.Dns;
using SonoffLanService.Devices;


var sd = new ServiceDiscovery();
sd.ServiceInstanceDiscovered += Device.ServiceInstanceDiscovered;

var tumbler = Device.Get<SwitchBasicR2>("1001666c98");
if (tumbler == null) return;

for (;;)
{
    Console.CursorLeft = 0;
    Console.Write("(Ctrl+C) exit; (1) on; (2) off, (3) toggle:  ");
    Console.CursorLeft -= 1;
    var k = Console.ReadKey();
    switch (k.Key)
    {
        case ConsoleKey.X:
            return;
        case ConsoleKey.D1:
            await tumbler.SwitchOn();
            break;
        case ConsoleKey.D2:
            await tumbler.SwitchOff();
            break;
        case ConsoleKey.D3:
            await tumbler.Toggle();
            break;
        default:
            continue;
    }
}