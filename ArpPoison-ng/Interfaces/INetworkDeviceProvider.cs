using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ArpPoison_ng.Interfaces
{
    public interface INetworkDeviceProvider
    {
        IReadOnlyList<ILiveDevice> GetDevices();
        ILiveDevice OpenDeviceByName(string deviceName);
        IPAddress? GetGatewayAddress();
        string? FormatMacAddress(PhysicalAddress? mac);
        IPAddress? GetLocalIP(ILiveDevice device);

    }
}
