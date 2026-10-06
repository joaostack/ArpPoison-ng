using ArpPoison_ng.Interfaces;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ArpPoison_ng.Services
{
    public class ArpPacketBuilder : IArpPacketBuilder
    {
        public ArpPacket BuildArp(PhysicalAddress senderMac, PhysicalAddress targetMac, IPAddress senderIp, IPAddress targetIp, ArpOperation operation)
        {
            var arp = new ArpPacket(
                operation,
                senderMac,
                senderIp,
                targetMac,
                targetIp);
            return arp;
        }

        public EthernetPacket BuildEthernet(PhysicalAddress sourceMac, PhysicalAddress destinationMac)
        {
            return new EthernetPacket(sourceMac, destinationMac, EthernetType.Arp);
        }
    }
}
