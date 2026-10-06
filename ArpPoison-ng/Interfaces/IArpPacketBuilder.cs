using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ArpPoison_ng.Interfaces
{
    public interface IArpPacketBuilder
    {
        EthernetPacket BuildEthernet(
            PhysicalAddress sourceMac,
            PhysicalAddress destinationMac);
        ArpPacket BuildArp(
            PhysicalAddress senderMac,
            PhysicalAddress targetMac,
            IPAddress senderIp,
            IPAddress targetIp,
            ArpOperation operation);
    }
}
