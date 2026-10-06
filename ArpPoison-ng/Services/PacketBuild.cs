using ArpPoison_ng.Interfaces;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ArpPoison_ng.Services
{
    public class PacketBuild : IPacketBuild
    {
        private readonly INetworkDeviceProvider _networkDeviceProvider;
        public PacketBuild(INetworkDeviceProvider networkDeviceProvider)
        {
            _networkDeviceProvider = networkDeviceProvider;
        }
        public async Task<string> GetMacAddress(ILiveDevice device, string ipAddress)
        {
            var localIp = _networkDeviceProvider.GetLocalIP(device);
            var localMac = device.MacAddress;
            var targetIp = IPAddress.Parse(ipAddress);
            var arpPacket = new ArpPacket(
                            ArpOperation.Request,
                            PhysicalAddress.Parse("00-00-00-00-00-00"),
                            targetIp,
                            localMac,
                            localIp);

            var ethernetPacket = new EthernetPacket(
                localMac,
                PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                EthernetType.Arp);

            ethernetPacket.PayloadPacket = arpPacket;

            string macRes = null;

            var tcs = new TaskCompletionSource<string>();

            PacketArrivalEventHandler handler = (object s, PacketCapture e) =>
            {
                var rawPacket = e.GetPacket();
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
                var arp = packet.Extract<ArpPacket>();

                if (arp != null && arp.Operation == ArpOperation.Response && arp.SenderProtocolAddress.Equals(targetIp))
                {
                    tcs.TrySetResult(arp.SenderHardwareAddress.ToString());
                }
            };

            device.Filter = "arp";
            device.OnPacketArrival += handler;
            device.StartCapture();

            await Task.Delay(500);

            device.SendPacket(ethernetPacket);

            try
            {
                macRes = await tcs.Task.WaitAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                macRes = null;
            }
            finally
            {
                device.OnPacketArrival -= handler;
            }

            return macRes ?? throw new InvalidOperationException($"[GetMacFromIP] MAC address not found for the target IP {ipAddress} (HOST DOWN)");
        }
    }
}
