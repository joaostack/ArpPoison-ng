using SharpPcap;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArpPoison_ng.Interfaces
{
    public interface IPacketBuild
    {
        Task<string> GetMacAddress(ILiveDevice device, string ipAddress);
    }
}
