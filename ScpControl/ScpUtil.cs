using System;
using System.Net.NetworkInformation;
using HidReport.Contract.Enums;
using ScpControl.Bluetooth;
using ScpControl.Shared.Core;

namespace ScpControl
{
    public interface IDsDevice
    {
        DsPadId PadId { get; set; }

        uint? XInputSlot { get; set; }

        DsConnection Connection { get; }

        DsState State { get; }

        DsBattery Battery { get; }

        DsModel Model { get; }

        PhysicalAddress DeviceAddress { get; }

        PhysicalAddress HostAddress { get; }

        bool Start();
        bool Rumble(byte large, byte small);
        bool Pair(PhysicalAddress master);
        bool Disconnect();
    }

    public interface IBthDevice
    {
        int HCI_Disconnect(BthHandle handle);
        int HID_Command(byte[] handle, byte[] channel, byte[] data);
    }

    public class DsNull : IDsDevice
    {
        public DsNull(DsPadId padId)
        {
            PadId = padId;
        }

        public DsPadId PadId { get; set; }

        public DsConnection Connection
        {
            get { return DsConnection.None; }
        }

        public DsState State
        {
            get { return DsState.Disconnected; }
        }

        public DsBattery Battery
        {
            get { return DsBattery.None; }
        }

        public DsModel Model
        {
            get { return DsModel.None; }
        }

        public PhysicalAddress DeviceAddress
        {
            get { return PhysicalAddress.None; }
        }

        public bool Start()
        {
            return true;
        }

        public bool Rumble(byte large, byte small)
        {
            return true;
        }

        public bool Disconnect()
        {
            return true;
        }

        public ScpHidReport NewHidReport()
        {
            return new ScpHidReport(DsConnection.None, DeviceAddress, DsModel.None, DsPadId.None, DsState.Connected, new HidReport.Core.HidReport());
        }

        public PhysicalAddress HostAddress
        {
            get { return PhysicalAddress.None; }
        }

        public bool Pair(PhysicalAddress master)
        {
            return true;
        }
        
        public uint? XInputSlot { get; set; }

        public override string ToString()
        {
            return string.Format("Pad {0} : {1}", 1 + (int) PadId, DsState.Disconnected);
        }
    }

    public class ArrivalEventArgs : EventArgs
    {
        public ArrivalEventArgs(IDsDevice device)
        {
            Device = device;
        }

        public IDsDevice Device { get; private set; }

        public bool Handled { get; set; }
    }
}
