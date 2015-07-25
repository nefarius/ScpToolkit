using System;
using System.ComponentModel;

using System.Collections.Generic;

namespace ScpControl 
{
    public partial class BusDevice : ScpDevice 
    {
        public const String SCP_BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";

        public const Int32 ReportSize = 28;
        public const Int32 RumbleSize =  8;
        public const Int32 BusWidth   =  4;

        protected DsState m_State = DsState.Disconnected;
        public DsState State 
        {
            get { return m_State; }
        }

        protected Int32 m_Offset = 0;
        protected List<Int32> m_Plugged = new List<Int32>();

        protected virtual Int32 IndexToSerial(Byte Index) 
        {
            return Index + m_Offset + 1;
        }

        public event EventHandler<DebugEventArgs> Debug = null;

        protected virtual void LogDebug(String Data) 
        {
            DebugEventArgs args = new DebugEventArgs(Data);

            if (Debug != null)
            {
                Debug(this, args);
            }
        }


        protected virtual Int32 Scale(Int32 Value, Boolean Flip) 
        {
            Value -= 0x80; if (Value == -128) Value = -127;

            if (Flip) Value *= -1;

            return (Int32)((float) Value * 258.00787401574803149606299212599f);
        }

        protected virtual Boolean DeadZone(Int32 R, Int32 X, Int32 Y) 
        {
            X -= 0x80; if (X == -128) X = -127;
            Y -= 0x80; if (Y == -128) Y = -127;

            return R * R >= X * X + Y * Y;
        }


        public BusDevice() : base(SCP_BUS_CLASS_GUID) 
        {
            InitializeComponent();
        }

        public BusDevice(IContainer container) : base(SCP_BUS_CLASS_GUID) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public override Boolean Open(Int32 Instance = 0) 
        {
            if (State == DsState.Disconnected)
            {
                m_Offset = Instance * BusWidth;

                LogDebug(String.Format("-- Bus Open   : Offset {0}", m_Offset));

                if (!base.Open(0))
                {
                    LogDebug(String.Format("-- Bus Open   : Failed!!", m_Offset));
                }
            }

            return State == DsState.Reserved;
        }

        public override Boolean Open(String DevicePath)  
        {
            if (State == DsState.Disconnected)
            {
                m_Path = DevicePath;

                LogDebug(String.Format("-- Bus Open   : Path {0}", m_Path));

                if (GetDeviceHandle(m_Path))
                {
                    m_IsActive = true;
                    m_State = DsState.Reserved;
                }
            }

            return State == DsState.Reserved;
        }

        public override Boolean Start() 
        {
            if (State == DsState.Reserved)
            {
                m_State = DsState.Connected;
            }

            return State == DsState.Connected;
        }

        public override Boolean Stop()  
        {
            if (State == DsState.Connected)
            {
                Queue<Int32> Items = new Queue<Int32>();

                lock (m_Plugged)
                {
                    foreach (Int32 Serial in m_Plugged) Items.Enqueue(Serial - m_Offset);
                }

                while (Items.Count > 0) Unplug(Items.Dequeue());

                m_State = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override Boolean Close() 
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


        public virtual Boolean Suspend() 
        {
            return Stop();
        }

        public virtual Boolean Resume()  
        {
            return Start();
        }


        public virtual Int32 Parse(Byte[] Input, Byte[] Output, DsModel Type = DsModel.DS3) 
        {
            Int32 Serial = IndexToSerial(Input[0]);

            for (Int32 Index = 0; Index < ReportSize; Index++) Output[Index] = 0x00;

            Output[0] = 0x1C;
            Output[4] = (Byte)((Serial >>  0) & 0xFF);
            Output[5] = (Byte)((Serial >>  8) & 0xFF);
            Output[6] = (Byte)((Serial >> 16) & 0xFF);
            Output[7] = (Byte)((Serial >> 24) & 0xFF);
            Output[9] = 0x14;

            X360Button XButton = X360Button.None;

            if (Input[1] == 0x02) // Pad is active
            {
                switch (Type)
                {
                    case DsModel.DS3:
                        {
                            Ds3Button Buttons = (Ds3Button)((Input[10] << 0) | (Input[11] << 8) | (Input[12] << 16) | (Input[13] << 24));

                            if (Buttons.HasFlag(Ds3Button.Select  )) XButton |= X360Button.Back; 
                            if (Buttons.HasFlag(Ds3Button.Start   )) XButton |= X360Button.Start;

                            if (Buttons.HasFlag(Ds3Button.Up      )) XButton |= X360Button.Up;
                            if (Buttons.HasFlag(Ds3Button.Right   )) XButton |= X360Button.Right;
                            if (Buttons.HasFlag(Ds3Button.Down    )) XButton |= X360Button.Down;
                            if (Buttons.HasFlag(Ds3Button.Left    )) XButton |= X360Button.Left;

                            if (Buttons.HasFlag(Ds3Button.L1      )) XButton |= X360Button.LB;
                            if (Buttons.HasFlag(Ds3Button.R1      )) XButton |= X360Button.RB;

                            if (Buttons.HasFlag(Ds3Button.Triangle)) XButton |= X360Button.Y;
                            if (Buttons.HasFlag(Ds3Button.Circle  )) XButton |= X360Button.B;
                            if (Buttons.HasFlag(Ds3Button.Cross   )) XButton |= X360Button.A;
                            if (Buttons.HasFlag(Ds3Button.Square  )) XButton |= X360Button.X;

                            if (Buttons.HasFlag(Ds3Button.PS      )) XButton |= X360Button.Guide;

                            if (Buttons.HasFlag(Ds3Button.L3      )) XButton |= X360Button.LS;
                            if (Buttons.HasFlag(Ds3Button.R3      )) XButton |= X360Button.RS;

                            Output[(UInt32) X360Axis.BT_Lo] = (Byte)((UInt32) XButton >> 0 & 0xFF);
                            Output[(UInt32) X360Axis.BT_Hi] = (Byte)((UInt32) XButton >> 8 & 0xFF);

                            Output[(UInt32) X360Axis.LT   ] = Input[(UInt32) Ds3Axis.L2];
                            Output[(UInt32) X360Axis.RT   ] = Input[(UInt32) Ds3Axis.R2];

                            if (!DeadZone(Global.DeadZoneL, Input[(UInt32) Ds3Axis.LX], Input[(UInt32) Ds3Axis.LY]))   // Left Stick DeadZone
                            {
                                Int32 ThumbLX = +Scale(Input[(UInt32) Ds3Axis.LX], Global.FlipLX);
                                Int32 ThumbLY = -Scale(Input[(UInt32) Ds3Axis.LY], Global.FlipLY);

                                Output[(UInt32) X360Axis.LX_Lo] = (Byte)((ThumbLX >> 0) & 0xFF); // LX
                                Output[(UInt32) X360Axis.LX_Hi] = (Byte)((ThumbLX >> 8) & 0xFF);

                                Output[(UInt32) X360Axis.LY_Lo] = (Byte)((ThumbLY >> 0) & 0xFF); // LY
                                Output[(UInt32) X360Axis.LY_Hi] = (Byte)((ThumbLY >> 8) & 0xFF);
                            }

                            if (!DeadZone(Global.DeadZoneR, Input[(UInt32) Ds3Axis.RX], Input[(UInt32) Ds3Axis.RY]))   // Right Stick DeadZone
                            {
                                Int32 ThumbRX = +Scale(Input[(UInt32) Ds3Axis.RX], Global.FlipRX);
                                Int32 ThumbRY = -Scale(Input[(UInt32) Ds3Axis.RY], Global.FlipRY);

                                Output[(UInt32) X360Axis.RX_Lo] = (Byte)((ThumbRX >> 0) & 0xFF); // RX
                                Output[(UInt32) X360Axis.RX_Hi] = (Byte)((ThumbRX >> 8) & 0xFF);

                                Output[(UInt32) X360Axis.RY_Lo] = (Byte)((ThumbRY >> 0) & 0xFF); // RY
                                Output[(UInt32) X360Axis.RY_Hi] = (Byte)((ThumbRY >> 8) & 0xFF);
                            }
                        }
                        break;

                    case DsModel.DS4:
                        {
                            Ds4Button Buttons = (Ds4Button)((Input[13] << 0) | (Input[14] << 8) | (Input[15] << 16));

                            if (Buttons.HasFlag(Ds4Button.Share   )) XButton |= X360Button.Back; 
                            if (Buttons.HasFlag(Ds4Button.Options )) XButton |= X360Button.Start;

                            if (Buttons.HasFlag(Ds4Button.Up      )) XButton |= X360Button.Up;
                            if (Buttons.HasFlag(Ds4Button.Right   )) XButton |= X360Button.Right;
                            if (Buttons.HasFlag(Ds4Button.Down    )) XButton |= X360Button.Down;
                            if (Buttons.HasFlag(Ds4Button.Left    )) XButton |= X360Button.Left;

                            if (Buttons.HasFlag(Ds4Button.L1      )) XButton |= X360Button.LB;
                            if (Buttons.HasFlag(Ds4Button.R1      )) XButton |= X360Button.RB;

                            if (Buttons.HasFlag(Ds4Button.Triangle)) XButton |= X360Button.Y;
                            if (Buttons.HasFlag(Ds4Button.Circle  )) XButton |= X360Button.B;
                            if (Buttons.HasFlag(Ds4Button.Cross   )) XButton |= X360Button.A;
                            if (Buttons.HasFlag(Ds4Button.Square  )) XButton |= X360Button.X;

                            if (Buttons.HasFlag(Ds4Button.PS      )) XButton |= X360Button.Guide;

                            if (Buttons.HasFlag(Ds4Button.L3      )) XButton |= X360Button.LS;
                            if (Buttons.HasFlag(Ds4Button.R3      )) XButton |= X360Button.RS;

                            Output[(UInt32) X360Axis.BT_Lo] = (Byte)((UInt32) XButton >> 0 & 0xFF);
                            Output[(UInt32) X360Axis.BT_Hi] = (Byte)((UInt32) XButton >> 8 & 0xFF);

                            Output[(UInt32) X360Axis.LT   ] = Input[(UInt32) Ds4Axis.L2];
                            Output[(UInt32) X360Axis.RT   ] = Input[(UInt32) Ds4Axis.R2];

                            if (!DeadZone(Global.DeadZoneL, Input[(UInt32) Ds4Axis.LX], Input[(UInt32) Ds4Axis.LY]))   // Left Stick DeadZone
                            {
                                Int32 ThumbLX = +Scale(Input[(UInt32) Ds4Axis.LX], Global.FlipLX);
                                Int32 ThumbLY = -Scale(Input[(UInt32) Ds4Axis.LY], Global.FlipLY);

                                Output[(UInt32) X360Axis.LX_Lo] = (Byte)((ThumbLX >> 0) & 0xFF); // LX
                                Output[(UInt32) X360Axis.LX_Hi] = (Byte)((ThumbLX >> 8) & 0xFF);

                                Output[(UInt32) X360Axis.LY_Lo] = (Byte)((ThumbLY >> 0) & 0xFF); // LY
                                Output[(UInt32) X360Axis.LY_Hi] = (Byte)((ThumbLY >> 8) & 0xFF);
                            }

                            if (!DeadZone(Global.DeadZoneR, Input[(UInt32) Ds4Axis.RX], Input[(UInt32) Ds4Axis.RY]))   // Right Stick DeadZone
                            {
                                Int32 ThumbRX = +Scale(Input[(UInt32) Ds4Axis.RX], Global.FlipRX);
                                Int32 ThumbRY = -Scale(Input[(UInt32) Ds4Axis.RY], Global.FlipRY);

                                Output[(UInt32) X360Axis.RX_Lo] = (Byte)((ThumbRX >> 0) & 0xFF); // RX
                                Output[(UInt32) X360Axis.RX_Hi] = (Byte)((ThumbRX >> 8) & 0xFF);

                                Output[(UInt32) X360Axis.RY_Lo] = (Byte)((ThumbRY >> 0) & 0xFF); // RY
                                Output[(UInt32) X360Axis.RY_Hi] = (Byte)((ThumbRY >> 8) & 0xFF);
                            }
                        }
                        break;
                }
            }

            return Input[0];
        }


        public virtual Boolean Plugin(Int32 Serial) 
        {
            Boolean retVal = false;

            if (Serial < 1 || Serial > BusWidth) return retVal;

            Serial += m_Offset;

            if (State == DsState.Connected)
            {
                lock (m_Plugged)
                {
                    if (!m_Plugged.Contains(Serial))
                    {
                        Int32 Transfered = 0;
                        Byte[] Buffer = new Byte[16];

                        Buffer[0] = 0x10;
                        Buffer[1] = 0x00;
                        Buffer[2] = 0x00;
                        Buffer[3] = 0x00;

                        Buffer[4] = (Byte)((Serial >>  0) & 0xFF);
                        Buffer[5] = (Byte)((Serial >>  8) & 0xFF);
                        Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
                        Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

                        if (DeviceIoControl(m_FileHandle, 0x2A4000, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero))
                        {
                            m_Plugged.Add(Serial); retVal = true;

                            LogDebug(String.Format("-- Bus Plugin : Serial {0}", Serial));
                        }
                    }
                    else retVal = true;
                }
            }

            return retVal;
        }

        public virtual Boolean Unplug(Int32 Serial) 
        {
            Boolean retVal = false;
            Serial += m_Offset;

            if (State == DsState.Connected)
            {
                lock (m_Plugged)
                {
                    if (m_Plugged.Contains(Serial))
                    {
                        Int32 Transfered = 0;
                        Byte[] Buffer = new Byte[16];

                        Buffer[0] = 0x10;
                        Buffer[1] = 0x00;
                        Buffer[2] = 0x00;
                        Buffer[3] = 0x00;

                        Buffer[4] = (Byte)((Serial >>  0) & 0xFF);
                        Buffer[5] = (Byte)((Serial >>  8) & 0xFF);
                        Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
                        Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

                        if (DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero))
                        {
                            m_Plugged.Remove(Serial); retVal = true;

                            LogDebug(String.Format("-- Bus Unplug : Serial {0}", Serial));
                        }
                    }
                    else retVal = true;
                }
            }

            return retVal;
        }

        public virtual Boolean Report(Byte[] Input, Byte[] Output) 
        {
            if (State == DsState.Connected)
            {
                Int32 Transfered = 0;

                return DeviceIoControl(m_FileHandle, 0x2A400C, Input, Input.Length, Output, Output.Length, ref Transfered, IntPtr.Zero) && Transfered > 0;
            }

            return false;
        }
    }
}
