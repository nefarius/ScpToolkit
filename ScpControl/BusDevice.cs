using System;
using System.Collections.Generic;
using System.ComponentModel;
using ScpControl.Profiler;
using ScpControl.ScpCore;

namespace ScpControl
{
    public sealed partial class BusDevice : ScpDevice
    {
        #region Private fields

        private const int BusWidth = 4;
        private readonly List<int> m_Plugged = new List<int>();
        private int m_Offset;
        private DsState m_State = DsState.Disconnected;

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
            get { return m_State; }
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

        #region Private methods

        private int IndexToSerial(byte index)
        {
            return index + m_Offset + 1;
        }

        /// <summary>
        ///     Translates DualShock axis value to Xbox 360 compatible value.
        /// </summary>
        /// <param name="value">The DualShock value.</param>
        /// <param name="flip">True to invert the axis, false for 1:1 scaling.</param>
        /// <returns>The Xbox 360 value.</returns>
        private static int Scale(int value, bool flip)
        {
            value -= 0x80;
            if (value == -128) value = -127;

            if (flip) value *= -1;

            return (int) (value*258.00787401574803149606299212599f);
        }

        /// <summary>
        ///     Checks if X and Y positions are within the provided dead zone.
        /// </summary>
        /// <param name="r">The threshold value.</param>
        /// <param name="x">The value for the X-axis.</param>
        /// <param name="y">The value for the Y-axis.</param>
        /// <returns>True if positions are within the dead zone, false otherwise.</returns>
        private static bool DeadZone(int r, int x, int y)
        {
            x -= 0x80;
            if (x == -128) x = -127;
            y -= 0x80;
            if (y == -128) y = -127;

            return r*r >= x*x + y*y;
        }

        #endregion

        #region Public methods

        public override bool Open(int instance = 0)
        {
            if (State == DsState.Disconnected)
            {
                m_Offset = instance*BusWidth;

                Log.DebugFormat("-- Bus Open: Offset {0}", m_Offset);

                if (!base.Open(0))
                {
                    Log.ErrorFormat("-- Bus Open: Offset {0} failed", m_Offset);
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
                    m_State = DsState.Reserved;
                }
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            if (State == DsState.Reserved)
            {
                m_State = DsState.Connected;
            }

            return State == DsState.Connected;
        }

        public override bool Stop()
        {
            if (State == DsState.Connected)
            {
                var Items = new Queue<int>();

                lock (m_Plugged)
                {
                    foreach (var serial in m_Plugged) Items.Enqueue(serial - m_Offset);
                }

                while (Items.Count > 0) Unplug(Items.Dequeue());

                m_State = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override bool Close()
        {
            if (base.Stop())
            {
                m_State = DsState.Reserved;
            }

            if (State != DsState.Reserved)
            {
                if (base.Close())
                {
                    m_State = DsState.Disconnected;
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

        public int Parse(ScpHidReport inputReport, byte[] output, DsModel type = DsModel.DS3)
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
                switch (type)
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

                        if (
                            !DeadZone(GlobalConfiguration.Instance.DeadZoneL, inputReport[Ds3Axis.Lx].Value,
                                inputReport[Ds3Axis.Ly].Value))
                            // Left Stick DeadZone
                        {
                            var thumbLx = +Scale(inputReport[Ds3Axis.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                            var thumbLy = -Scale(inputReport[Ds3Axis.Ly].Value, GlobalConfiguration.Instance.FlipLY);

                            output[(uint) X360Axis.LX_Lo] = (byte) ((thumbLx >> 0) & 0xFF); // LX
                            output[(uint) X360Axis.LX_Hi] = (byte) ((thumbLx >> 8) & 0xFF);

                            output[(uint) X360Axis.LY_Lo] = (byte) ((thumbLy >> 0) & 0xFF); // LY
                            output[(uint) X360Axis.LY_Hi] = (byte) ((thumbLy >> 8) & 0xFF);
                        }

                        if (
                            !DeadZone(GlobalConfiguration.Instance.DeadZoneR, inputReport[Ds3Axis.Rx].Value,
                                inputReport[Ds3Axis.Ry].Value))
                            // Right Stick DeadZone
                        {
                            var thumbRx = +Scale(inputReport[Ds3Axis.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                            var thumbRy = -Scale(inputReport[Ds3Axis.Ry].Value, GlobalConfiguration.Instance.FlipRY);

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

                        if (
                            !DeadZone(GlobalConfiguration.Instance.DeadZoneL, inputReport[Ds4Axis.Lx].Value,
                                inputReport[Ds4Axis.Ly].Value))
                            // Left Stick DeadZone
                        {
                            var thumbLx = +Scale(inputReport[Ds4Axis.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                            var thumbLy = -Scale(inputReport[Ds4Axis.Ly].Value, GlobalConfiguration.Instance.FlipLY);

                            output[(uint) X360Axis.LX_Lo] = (byte) ((thumbLx >> 0) & 0xFF); // LX
                            output[(uint) X360Axis.LX_Hi] = (byte) ((thumbLx >> 8) & 0xFF);

                            output[(uint) X360Axis.LY_Lo] = (byte) ((thumbLy >> 0) & 0xFF); // LY
                            output[(uint) X360Axis.LY_Hi] = (byte) ((thumbLy >> 8) & 0xFF);
                        }

                        if (
                            !DeadZone(GlobalConfiguration.Instance.DeadZoneR, inputReport[Ds4Axis.Rx].Value,
                                inputReport[Ds4Axis.Ry].Value))
                            // Right Stick DeadZone
                        {
                            var thumbRx = +Scale(inputReport[Ds4Axis.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                            var thumbRy = -Scale(inputReport[Ds4Axis.Ry].Value, GlobalConfiguration.Instance.FlipRY);

                            output[(uint) X360Axis.RX_Lo] = (byte) ((thumbRx >> 0) & 0xFF); // RX
                            output[(uint) X360Axis.RX_Hi] = (byte) ((thumbRx >> 8) & 0xFF);

                            output[(uint) X360Axis.RY_Lo] = (byte) ((thumbRy >> 0) & 0xFF); // RY
                            output[(uint) X360Axis.RY_Hi] = (byte) ((thumbRy >> 8) & 0xFF);
                        }
                    }
                        break;
                }
            }

            return input[0];
        }

        public bool Plugin(int serial)
        {
            var retVal = false;

            if (serial < 1 || serial > BusWidth) return retVal;

            serial += m_Offset;

            if (State == DsState.Connected)
            {
                lock (m_Plugged)
                {
                    if (!m_Plugged.Contains(serial))
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
                            m_Plugged.Add(serial);
                            retVal = true;

                            Log.DebugFormat("-- Bus Plugin : Serial {0}", serial);
                        }
                    }
                    else retVal = true;
                }
            }

            return retVal;
        }

        public bool Unplug(int serial)
        {
            var retVal = false;
            serial += m_Offset;

            if (State == DsState.Connected)
            {
                lock (m_Plugged)
                {
                    if (m_Plugged.Contains(serial))
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
                            m_Plugged.Remove(serial);
                            retVal = true;

                            Log.DebugFormat("-- Bus Unplug : Serial {0}", serial);
                        }
                    }
                    else retVal = true;
                }
            }

            return retVal;
        }

        public bool Report(byte[] input, byte[] output)
        {
            if (State == DsState.Connected)
            {
                var transfered = 0;

                return
                    DeviceIoControl(FileHandle, 0x2A400C, input, input.Length, output, output.Length, ref transfered,
                        IntPtr.Zero) && transfered > 0;
            }

            return false;
        }

        #endregion
    }
}
