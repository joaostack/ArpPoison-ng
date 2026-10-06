using SharpPcap;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArpPoison_ng.Interfaces
{
    public interface IArpPoison
    {
        void Spoof(ILiveDevice device, string targetIp, string gatewayIp, string targetMac, string gatewayMac);
    }
}
