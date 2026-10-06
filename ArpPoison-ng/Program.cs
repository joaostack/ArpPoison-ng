using ArpPoison_ng.Commands;
using ArpPoison_ng.DI;
using ArpPoison_ng.Interfaces;
using ArpPoison_ng.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();

services.AddSingleton<IConsoleUI, ConsoleUI>();
services.AddSingleton<INetworkDeviceProvider, NetworkDeviceProvider>();
services.AddSingleton<IArpPacketBuilder, ArpPacketBuilder>();
services.AddSingleton<IPacketBuild, PacketBuild>();
services.AddSingleton<IArpPoison, ArpPoison>();

var registrar = new TypeRegistrar(services);

var app = new CommandApp<StartCommand>(registrar);

return app.Run(args);