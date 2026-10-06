using ArpPoison_ng.Interfaces;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ArpPoison_ng.Services
{
    public class NetworkDeviceProvider : INetworkDeviceProvider
    {
        public string? FormatMacAddress(PhysicalAddress? mac)
        {
            if (mac is null)
                return null;

            return string.Join(
                ":",
                mac.GetAddressBytes()
                    .Select(b => b.ToString("X2")));
        }

        public ILiveDevice GetDevice(int index)
        {
            var devices = GetDevices();

            if (devices.Count == 0)
                throw new InvalidOperationException(
                    "No network devices found.");

            if (index < 0 || index >= devices.Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "Invalid network device index.");

            return devices[index];
        }

        public IReadOnlyList<ILiveDevice> GetDevices()
        {
            return CaptureDeviceList.Instance;
        }

        public IPAddress? GetGatewayAddress()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up)
                .SelectMany(n =>
                    n.GetIPProperties().GatewayAddresses)
                .Select(g => g.Address)
                .FirstOrDefault(a =>
                    a.AddressFamily == AddressFamily.InterNetwork);
        }

        public IPAddress? GetLocalIP(ILiveDevice device)
        {
            var localIp = ((SharpPcap.LibPcap.LibPcapLiveDevice)device).Addresses
                .FirstOrDefault(a =>
                    a.Addr.ipAddress != null &&
                    a.Addr.ipAddress.AddressFamily == AddressFamily.InterNetwork)
                ?.Addr.ipAddress;

            return localIp;
        }

        public ILiveDevice OpenDeviceByName(string deviceName)
        {
            LibPcapLiveDeviceList deviceList = LibPcapLiveDeviceList.Instance;
            var device = deviceList.FirstOrDefault(dev => dev.Interface?.FriendlyName?.ToLower() == deviceName);
            if (device is null)
                throw new NullReferenceException("Device not found!");

            device.Open(DeviceModes.Promiscuous, 1000);

            return device;
        }
    }
}
