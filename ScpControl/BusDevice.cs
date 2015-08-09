using System;
using System.Collections.Generic;
using System.ComponentModel;
using ScpControl.ScpCore;

namespace ScpControl
{
    public sealed partial class BusDevice : ScpDevice
    {
        private const string SCP_BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";
        public const int ReportSize = 28;
        public const int RumbleSize = 8;
        private const int BusWidth = 4;
        private readonly List<int> m_Plugged = new List<int>();
        private int m_Offset;
        private DsState m_State = DsState.Disconnected;

        public BusDevice() : base(SCP_BUS_CLASS_GUID)
        {
            InitializeComponent();
        }

        public BusDevice(IContainer container) : base(SCP_BUS_CLASS_GUID)
        {
            container.Add(this);

            InitializeComponent();
        }

        public DsState State
        {
            get { return m_State; }
        }

        private int IndexToSerial(byte Index)
        {
            return Index + m_Offset + 1;
        }

        private int Scale(int Value, bool Flip)
        {
            Value -= 0x80;
            if (Value == -128) Value = -127;

            if (Flip) Value *= -1;

            return (int) (Value*258.00787401574803149606299212599f);
        }

        private bool DeadZone(int R, int X, int Y)
        {
            X -= 0x80;
            if (X == -128) X = -127;
            Y -= 0x80;
            if (Y == -128) Y = -127;

            return R*R >= X*X + Y*Y;
        }

        public override bool Open(int instance = 0)
        {
            if (State == DsState.Disconnected)
            {
                m_Offset = instance*BusWidth;

                Log.DebugFormat("-- Bus Open   : Offset {0}", m_Offset);

                if (!base.Open(0))
                {
                    Log.DebugFormat("-- Bus Open   : Failed!!", m_Offset);
                }
            }

            return State == DsState.Reserved;
        }

        public override bool Open(string devicePath)
        {
            if (State == DsState.Disconnected)
            {
                m_Path = devicePath;

                Log.DebugFormat("-- Bus Open   : Path {0}", m_Path);

                if (GetDeviceHandle(m_Path))
                {
                    m_IsActive = true;
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

        public int Parse(byte[] Input, byte[] Output, DsModel Type = DsModel.DS3)
        {
            var serial = IndexToSerial(Input[0]);

            for (var index = 0; index < ReportSize; index++) Output[index] = 0x00;

            Output[0] = 0x1C;
            Output[4] = (byte) ((serial >> 0) & 0xFF);
            Output[5] = (byte) ((serial >> 8) & 0xFF);
            Output[6] = (byte) ((serial >> 16) & 0xFF);
            Output[7] = (byte) ((serial >> 24) & 0xFF);
            Output[9] = 0x14;

            var xButton = X360Button.None;

            if (Input[1] == 0x02) // Pad is active
            {
                switch (Type)
                {
                    case DsModel.DS3:
                    {
                        var buttons =
                            (Ds3Button) ((Input[10] << 0) | (Input[11] << 8) | (Input[12] << 16) | (Input[13] << 24));

                        if (buttons.HasFlag(Ds3Button.Select)) xButton |= X360Button.Back;
                        if (buttons.HasFlag(Ds3Button.Start)) xButton |= X360Button.Start;

                        if (buttons.HasFlag(Ds3Button.Up)) xButton |= X360Button.Up;
                        if (buttons.HasFlag(Ds3Button.Right)) xButton |= X360Button.Right;
                        if (buttons.HasFlag(Ds3Button.Down)) xButton |= X360Button.Down;
                        if (buttons.HasFlag(Ds3Button.Left)) xButton |= X360Button.Left;

                        if (buttons.HasFlag(Ds3Button.L1)) xButton |= X360Button.LB;
                        if (buttons.HasFlag(Ds3Button.R1)) xButton |= X360Button.RB;

                        if (buttons.HasFlag(Ds3Button.Triangle)) xButton |= X360Button.Y;
                        if (buttons.HasFlag(Ds3Button.Circle)) xButton |= X360Button.B;
                        if (buttons.HasFlag(Ds3Button.Cross)) xButton |= X360Button.A;
                        if (buttons.HasFlag(Ds3Button.Square)) xButton |= X360Button.X;

                        if (buttons.HasFlag(Ds3Button.PS)) xButton |= X360Button.Guide;

                        if (buttons.HasFlag(Ds3Button.L3)) xButton |= X360Button.LS;
                        if (buttons.HasFlag(Ds3Button.R3)) xButton |= X360Button.RS;

                        Output[(uint) X360Axis.BT_Lo] = (byte) ((uint) xButton >> 0 & 0xFF);
                        Output[(uint) X360Axis.BT_Hi] = (byte) ((uint) xButton >> 8 & 0xFF);

                        Output[(uint) X360Axis.LT] = Input[(uint) Ds3Axis.L2];
                        Output[(uint) X360Axis.RT] = Input[(uint) Ds3Axis.R2];

                        if (!DeadZone(GlobalConfiguration.Instance.DeadZoneL, Input[(uint)Ds3Axis.LX], Input[(uint)Ds3Axis.LY]))
                            // Left Stick DeadZone
                        {
                            var thumbLx = +Scale(Input[(uint)Ds3Axis.LX], GlobalConfiguration.Instance.FlipLX);
                            var thumbLy = -Scale(Input[(uint)Ds3Axis.LY], GlobalConfiguration.Instance.FlipLY);

                            Output[(uint) X360Axis.LX_Lo] = (byte) ((thumbLx >> 0) & 0xFF); // LX
                            Output[(uint) X360Axis.LX_Hi] = (byte) ((thumbLx >> 8) & 0xFF);

                            Output[(uint) X360Axis.LY_Lo] = (byte) ((thumbLy >> 0) & 0xFF); // LY
                            Output[(uint) X360Axis.LY_Hi] = (byte) ((thumbLy >> 8) & 0xFF);
                        }

                        if (!DeadZone(GlobalConfiguration.Instance.DeadZoneR, Input[(uint)Ds3Axis.RX], Input[(uint)Ds3Axis.RY]))
                            // Right Stick DeadZone
                        {
                            var thumbRx = +Scale(Input[(uint)Ds3Axis.RX], GlobalConfiguration.Instance.FlipRX);
                            var thumbRy = -Scale(Input[(uint)Ds3Axis.RY], GlobalConfiguration.Instance.FlipRY);

                            Output[(uint) X360Axis.RX_Lo] = (byte) ((thumbRx >> 0) & 0xFF); // RX
                            Output[(uint) X360Axis.RX_Hi] = (byte) ((thumbRx >> 8) & 0xFF);

                            Output[(uint) X360Axis.RY_Lo] = (byte) ((thumbRy >> 0) & 0xFF); // RY
                            Output[(uint) X360Axis.RY_Hi] = (byte) ((thumbRy >> 8) & 0xFF);
                        }
                    }
                        break;

                    case DsModel.DS4:
                    {
                        var buttons = (Ds4Button) ((Input[13] << 0) | (Input[14] << 8) | (Input[15] << 16));

                        if (buttons.HasFlag(Ds4Button.Share)) xButton |= X360Button.Back;
                        if (buttons.HasFlag(Ds4Button.Options)) xButton |= X360Button.Start;

                        if (buttons.HasFlag(Ds4Button.Up)) xButton |= X360Button.Up;
                        if (buttons.HasFlag(Ds4Button.Right)) xButton |= X360Button.Right;
                        if (buttons.HasFlag(Ds4Button.Down)) xButton |= X360Button.Down;
                        if (buttons.HasFlag(Ds4Button.Left)) xButton |= X360Button.Left;

                        if (buttons.HasFlag(Ds4Button.L1)) xButton |= X360Button.LB;
                        if (buttons.HasFlag(Ds4Button.R1)) xButton |= X360Button.RB;

                        if (buttons.HasFlag(Ds4Button.Triangle)) xButton |= X360Button.Y;
                        if (buttons.HasFlag(Ds4Button.Circle)) xButton |= X360Button.B;
                        if (buttons.HasFlag(Ds4Button.Cross)) xButton |= X360Button.A;
                        if (buttons.HasFlag(Ds4Button.Square)) xButton |= X360Button.X;

                        if (buttons.HasFlag(Ds4Button.PS)) xButton |= X360Button.Guide;

                        if (buttons.HasFlag(Ds4Button.L3)) xButton |= X360Button.LS;
                        if (buttons.HasFlag(Ds4Button.R3)) xButton |= X360Button.RS;

                        Output[(uint) X360Axis.BT_Lo] = (byte) ((uint) xButton >> 0 & 0xFF);
                        Output[(uint) X360Axis.BT_Hi] = (byte) ((uint) xButton >> 8 & 0xFF);

                        Output[(uint) X360Axis.LT] = Input[(uint) Ds4Axis.L2];
                        Output[(uint) X360Axis.RT] = Input[(uint) Ds4Axis.R2];

                        if (!DeadZone(GlobalConfiguration.Instance.DeadZoneL, Input[(uint)Ds4Axis.LX], Input[(uint)Ds4Axis.LY]))
                            // Left Stick DeadZone
                        {
                            var thumbLx = +Scale(Input[(uint)Ds4Axis.LX], GlobalConfiguration.Instance.FlipLX);
                            var thumbLy = -Scale(Input[(uint)Ds4Axis.LY], GlobalConfiguration.Instance.FlipLY);

                            Output[(uint) X360Axis.LX_Lo] = (byte) ((thumbLx >> 0) & 0xFF); // LX
                            Output[(uint) X360Axis.LX_Hi] = (byte) ((thumbLx >> 8) & 0xFF);

                            Output[(uint) X360Axis.LY_Lo] = (byte) ((thumbLy >> 0) & 0xFF); // LY
                            Output[(uint) X360Axis.LY_Hi] = (byte) ((thumbLy >> 8) & 0xFF);
                        }

                        if (!DeadZone(GlobalConfiguration.Instance.DeadZoneR, Input[(uint)Ds4Axis.RX], Input[(uint)Ds4Axis.RY]))
                            // Right Stick DeadZone
                        {
                            var thumbRx = +Scale(Input[(uint)Ds4Axis.RX], GlobalConfiguration.Instance.FlipRX);
                            var thumbRy = -Scale(Input[(uint)Ds4Axis.RY], GlobalConfiguration.Instance.FlipRY);

                            Output[(uint) X360Axis.RX_Lo] = (byte) ((thumbRx >> 0) & 0xFF); // RX
                            Output[(uint) X360Axis.RX_Hi] = (byte) ((thumbRx >> 8) & 0xFF);

                            Output[(uint) X360Axis.RY_Lo] = (byte) ((thumbRy >> 0) & 0xFF); // RY
                            Output[(uint) X360Axis.RY_Hi] = (byte) ((thumbRy >> 8) & 0xFF);
                        }
                    }
                        break;
                }
            }

            return Input[0];
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

                        if (DeviceIoControl(m_FileHandle, 0x2A4000, buffer, buffer.Length, null, 0, ref transfered,
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

                        if (DeviceIoControl(m_FileHandle, 0x2A4004, buffer, buffer.Length, null, 0, ref transfered,
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
                    DeviceIoControl(m_FileHandle, 0x2A400C, input, input.Length, output, output.Length, ref transfered,
                        IntPtr.Zero) && transfered > 0;
            }

            return false;
        }
    }
}