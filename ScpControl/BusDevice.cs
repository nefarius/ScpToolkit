using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Shared.Utilities;
using ScpControl.Shared.XInput;
using ScpControl.XInput;

namespace ScpControl
{
    public sealed partial class BusDevice : ScpDevice
    {
        #region Private fields

        private const int BusWidth = 4;
        private readonly List<int> _pluggedInDevices = new List<int>();
        private int _busOffset;
        private DsState _busState = DsState.Disconnected;

        #endregion
        
        #region Public properties

        public static int ReportSize
        {
            get { return 28; }
        }

        public static int RumbleSize
        {
            get { return 8; }
        }

        private static Guid DeviceClassGuid
        {
            get { return Guid.Parse("{F679F562-3164-42CE-A4DB-E7DDBE723909}"); }
        }

        public DsState State
        {
            get { return _busState; }
        }

        #endregion

        #region Ctors

        public BusDevice() : base(DeviceClassGuid)
        {
            InitializeComponent();
        }

        public BusDevice(IContainer container) : base(DeviceClassGuid)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Translates Pad ID to bus device offset.
        /// </summary>
        /// <param name="index">The Pad ID to translate.</param>
        /// <returns>The bus device serial.</returns>
        public int IndexToSerial(byte index)
        {
            return index + _busOffset + 1;
        }

        public override bool Open(int instance = 0)
        {
            if (State == DsState.Disconnected)
            {
                _busOffset = instance*BusWidth;

                Log.DebugFormat("-- Bus Open: Offset {0}", _busOffset);

                if (!base.Open(0))
                {
                    Log.ErrorFormat("-- Bus Open: Offset {0} failed", _busOffset);
                }
            }

            return State == DsState.Reserved;
        }

        public override bool Open(string devicePath)
        {
            if (State == DsState.Disconnected)
            {
                Path = devicePath;

                Log.DebugFormat("-- Bus Open: Path {0}", Path);

                if (GetDeviceHandle(Path))
                {
                    IsActive = true;
                    _busState = DsState.Reserved;
                }
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            if (State == DsState.Reserved)
            {
                _busState = DsState.Connected;
            }

            return State == DsState.Connected;
        }

        public override bool Stop()
        {
            if (State == DsState.Connected)
            {
                var Items = new Queue<int>();

                lock (_pluggedInDevices)
                {
                    foreach (var serial in _pluggedInDevices) Items.Enqueue(serial - _busOffset);
                }

                while (Items.Count > 0) Unplug(Items.Dequeue());

                _busState = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override bool Close()
        {
            if (base.Stop())
            {
                _busState = DsState.Reserved;
            }

            if (State != DsState.Reserved)
            {
                if (base.Close())
                {
                    _busState = DsState.Disconnected;
                }
            }

            return State == DsState.Disconnected;
        }

        public bool Suspend()
        {
            return Stop();
        }

        public bool Resume()
        {
            return Start();
        }

        /// <summary>
        ///     Translates an <see cref="ScpHidReport"/> to an Xbox 360 compatible byte array.
        /// </summary>
        /// <param name="inputReport">The <see cref="ScpHidReport"/> to translate.</param>
        /// <param name="output">The target Xbox data array.</param>
        public void Parse(ScpHidReport inputReport, byte[] output)
        {
            var input = inputReport.RawBytes;

            var serial = IndexToSerial(input[0]);

            for (var index = 0; index < ReportSize; index++) output[index] = 0x00;

            output[0] = 0x1C;
            output[4] = (byte) ((serial >> 0) & 0xFF);
            output[5] = (byte) ((serial >> 8) & 0xFF);
            output[6] = (byte) ((serial >> 16) & 0xFF);
            output[7] = (byte) ((serial >> 24) & 0xFF);
            output[9] = 0x14;

            var xButton = X360Button.None;

            if (inputReport.PadState == DsState.Connected) // Pad is active
            {
                switch (inputReport.Model)
                {
                    case DsModel.DS3:
                    {
                        // select & start
                        if (inputReport[Ds3Button.Select].IsPressed) xButton |= X360Button.Back;
                        if (inputReport[Ds3Button.Start].IsPressed) xButton |= X360Button.Start;

                        // d-pad
                        if (inputReport[Ds3Button.Up].IsPressed) xButton |= X360Button.Up;
                        if (inputReport[Ds3Button.Right].IsPressed) xButton |= X360Button.Right;
                        if (inputReport[Ds3Button.Down].IsPressed) xButton |= X360Button.Down;
                        if (inputReport[Ds3Button.Left].IsPressed) xButton |= X360Button.Left;

                        // shoulders
                        if (inputReport[Ds3Button.L1].IsPressed) xButton |= X360Button.LB;
                        if (inputReport[Ds3Button.R1].IsPressed) xButton |= X360Button.RB;

                        // face buttons
                        if (inputReport[Ds3Button.Triangle].IsPressed) xButton |= X360Button.Y;
                        if (inputReport[Ds3Button.Circle].IsPressed) xButton |= X360Button.B;
                        if (inputReport[Ds3Button.Cross].IsPressed) xButton |= X360Button.A;
                        if (inputReport[Ds3Button.Square].IsPressed) xButton |= X360Button.X;

                        // PS/Guide
                        if (inputReport[Ds3Button.Ps].IsPressed) xButton |= X360Button.Guide;

                        // thumbs
                        if (inputReport[Ds3Button.L3].IsPressed) xButton |= X360Button.LS;
                        if (inputReport[Ds3Button.R3].IsPressed) xButton |= X360Button.RS;

                        output[(uint) X360Axis.BT_Lo] = (byte) ((uint) xButton >> 0 & 0xFF);
                        output[(uint) X360Axis.BT_Hi] = (byte) ((uint) xButton >> 8 & 0xFF);

                        // trigger
                        output[(uint) X360Axis.LT] = inputReport[Ds3Axis.L2].Value;
                        output[(uint) X360Axis.RT] = inputReport[Ds3Axis.R2].Value;

                        if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneL, 
                            inputReport[Ds3Axis.Lx].Value,
                            inputReport[Ds3Axis.Ly].Value))
                            // Left Stick DeadZone
                        {
                            var thumbLx = +DsMath.Scale(inputReport[Ds3Axis.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                            var thumbLy = -DsMath.Scale(inputReport[Ds3Axis.Ly].Value, GlobalConfiguration.Instance.FlipLY);

                            output[(uint) X360Axis.LX_Lo] = (byte) ((thumbLx >> 0) & 0xFF); // LX
                            output[(uint) X360Axis.LX_Hi] = (byte) ((thumbLx >> 8) & 0xFF);

                            output[(uint) X360Axis.LY_Lo] = (byte) ((thumbLy >> 0) & 0xFF); // LY
                            output[(uint) X360Axis.LY_Hi] = (byte) ((thumbLy >> 8) & 0xFF);
                        }

                        if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneR,
                            inputReport[Ds3Axis.Rx].Value,
                            inputReport[Ds3Axis.Ry].Value))
                            // Right Stick DeadZone
                        {
                            var thumbRx = +DsMath.Scale(inputReport[Ds3Axis.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                            var thumbRy = -DsMath.Scale(inputReport[Ds3Axis.Ry].Value, GlobalConfiguration.Instance.FlipRY);

                            output[(uint) X360Axis.RX_Lo] = (byte) ((thumbRx >> 0) & 0xFF); // RX
                            output[(uint) X360Axis.RX_Hi] = (byte) ((thumbRx >> 8) & 0xFF);

                            output[(uint) X360Axis.RY_Lo] = (byte) ((thumbRy >> 0) & 0xFF); // RY
                            output[(uint) X360Axis.RY_Hi] = (byte) ((thumbRy >> 8) & 0xFF);
                        }
                    }
                        break;

                    case DsModel.DS4:
                    {
                        if (inputReport[Ds4Button.Share].IsPressed) xButton |= X360Button.Back;
                        if (inputReport[Ds4Button.Options].IsPressed) xButton |= X360Button.Start;

                        if (inputReport[Ds4Button.Up].IsPressed) xButton |= X360Button.Up;
                        if (inputReport[Ds4Button.Right].IsPressed) xButton |= X360Button.Right;
                        if (inputReport[Ds4Button.Down].IsPressed) xButton |= X360Button.Down;
                        if (inputReport[Ds4Button.Left].IsPressed) xButton |= X360Button.Left;

                        if (inputReport[Ds4Button.L1].IsPressed) xButton |= X360Button.LB;
                        if (inputReport[Ds4Button.R1].IsPressed) xButton |= X360Button.RB;

                        if (inputReport[Ds4Button.Triangle].IsPressed) xButton |= X360Button.Y;
                        if (inputReport[Ds4Button.Circle].IsPressed) xButton |= X360Button.B;
                        if (inputReport[Ds4Button.Cross].IsPressed) xButton |= X360Button.A;
                        if (inputReport[Ds4Button.Square].IsPressed) xButton |= X360Button.X;

                        if (inputReport[Ds4Button.Ps].IsPressed) xButton |= X360Button.Guide;

                        if (inputReport[Ds4Button.L3].IsPressed) xButton |= X360Button.LS;
                        if (inputReport[Ds4Button.R3].IsPressed) xButton |= X360Button.RS;

                        output[(uint) X360Axis.BT_Lo] = (byte) ((uint) xButton >> 0 & 0xFF);
                        output[(uint) X360Axis.BT_Hi] = (byte) ((uint) xButton >> 8 & 0xFF);

                        output[(uint) X360Axis.LT] = inputReport[Ds4Axis.L2].Value;
                        output[(uint) X360Axis.RT] = inputReport[Ds4Axis.R2].Value;

                        if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneL, 
                            inputReport[Ds4Axis.Lx].Value,
                            inputReport[Ds4Axis.Ly].Value))
                            // Left Stick DeadZone
                        {
                            var thumbLx = +DsMath.Scale(inputReport[Ds4Axis.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                            var thumbLy = -DsMath.Scale(inputReport[Ds4Axis.Ly].Value, GlobalConfiguration.Instance.FlipLY);

                            output[(uint) X360Axis.LX_Lo] = (byte) ((thumbLx >> 0) & 0xFF); // LX
                            output[(uint) X360Axis.LX_Hi] = (byte) ((thumbLx >> 8) & 0xFF);

                            output[(uint) X360Axis.LY_Lo] = (byte) ((thumbLy >> 0) & 0xFF); // LY
                            output[(uint) X360Axis.LY_Hi] = (byte) ((thumbLy >> 8) & 0xFF);
                        }

                        if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneR, 
                            inputReport[Ds4Axis.Rx].Value,
                            inputReport[Ds4Axis.Ry].Value))
                            // Right Stick DeadZone
                        {
                            var thumbRx = +DsMath.Scale(inputReport[Ds4Axis.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                            var thumbRy = -DsMath.Scale(inputReport[Ds4Axis.Ry].Value, GlobalConfiguration.Instance.FlipRY);

                            output[(uint) X360Axis.RX_Lo] = (byte) ((thumbRx >> 0) & 0xFF); // RX
                            output[(uint) X360Axis.RX_Hi] = (byte) ((thumbRx >> 8) & 0xFF);

                            output[(uint) X360Axis.RY_Lo] = (byte) ((thumbRy >> 0) & 0xFF); // RY
                            output[(uint) X360Axis.RY_Hi] = (byte) ((thumbRy >> 8) & 0xFF);
                        }
                    }
                        break;
                }
            }
        }

        public bool Plugin(int serial)
        {
            if (GlobalConfiguration.Instance.IsVBusDisabled) return true;

            var retVal = false;

            if (GlobalConfiguration.Instance.SkipOccupiedSlots)
            {
                while (IsSerialOccupied(serial) && serial < BusWidth)
                {
                    serial++;
                }
            }

            if (serial < 1 || serial > BusWidth) return false;

            serial += _busOffset;

            if (State != DsState.Connected) return false;

            lock (_pluggedInDevices)
            {
                if (!_pluggedInDevices.Contains(serial))
                {
                    var transfered = 0;
                    var buffer = new byte[16];

                    buffer[0] = 0x10;
                    buffer[1] = 0x00;
                    buffer[2] = 0x00;
                    buffer[3] = 0x00;

                    buffer[4] = (byte) ((serial >> 0) & 0xFF);
                    buffer[5] = (byte) ((serial >> 8) & 0xFF);
                    buffer[6] = (byte) ((serial >> 16) & 0xFF);
                    buffer[7] = (byte) ((serial >> 24) & 0xFF);

                    if (DeviceIoControl(FileHandle, 0x2A4000, buffer, buffer.Length, null, 0, ref transfered,
                        IntPtr.Zero))
                    {
                        _pluggedInDevices.Add(serial);
                        retVal = true;

                        Log.DebugFormat("-- Bus Plugin : Serial {0}", serial);
                    }
                    else
                    {
                        Log.ErrorFormat("Couldn't plug in virtual device {0}: {1}", serial,
                            new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }
                else retVal = true;
            }

            return retVal;
        }

        public bool Unplug(int serial)
        {
            if (GlobalConfiguration.Instance.IsVBusDisabled) return true;

            var retVal = false;
            serial += _busOffset;

            if (State != DsState.Connected) return false;

            lock (_pluggedInDevices)
            {
                if (_pluggedInDevices.Contains(serial))
                {
                    var transfered = 0;
                    var buffer = new byte[16];

                    buffer[0] = 0x10;
                    buffer[1] = 0x00;
                    buffer[2] = 0x00;
                    buffer[3] = 0x00;

                    buffer[4] = (byte) ((serial >> 0) & 0xFF);
                    buffer[5] = (byte) ((serial >> 8) & 0xFF);
                    buffer[6] = (byte) ((serial >> 16) & 0xFF);
                    buffer[7] = (byte) ((serial >> 24) & 0xFF);

                    if (DeviceIoControl(FileHandle, 0x2A4004, buffer, buffer.Length, null, 0, ref transfered,
                        IntPtr.Zero))
                    {
                        _pluggedInDevices.Remove(serial);
                        retVal = true;

                        Log.DebugFormat("-- Bus Unplug : Serial {0}", serial);
                    }
                    else
                    {
                        Log.ErrorFormat("Couldn't unplug virtual device {0}: {1}", serial,
                            new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }
                else retVal = true;
            }

            return retVal;
        }

        /// <summary>
        ///     Sends a supplied Xbox formatted report to the virtual bus.
        /// </summary>
        /// <param name="input">The data to send to the bus device.</param>
        /// <param name="output">The data reported back by the bus device.</param>
        /// <returns>True if the I/O request was successful, false otherwise.</returns>
        public bool Report(byte[] input, byte[] output)
        {
            if (State != DsState.Connected) return false;

            var transfered = 0;

            return
                DeviceIoControl(FileHandle, 0x2A400C, input, input.Length, output, output.Length, ref transfered,
                    IntPtr.Zero) && transfered > 0;
        }

        #endregion

        #region Private methods

        private static bool IsSerialOccupied(int serial)
        {
            if (--serial < 0 || serial > 3)
            {
                throw new ArgumentException(string.Format("Serial index ({0}) must be within range", serial));
            }

            var state = new XINPUT_STATE();
            return (XInputNatives.XInputGetState((uint) serial, ref state) == ResultWin32.ERROR_SUCCESS);
        }

        #endregion
    }
}
