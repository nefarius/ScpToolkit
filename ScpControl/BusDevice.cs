using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ScpControl.Driver;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Shared.Utilities;
using ScpControl.Shared.XInput;

namespace ScpControl
{
    public class BusDevice : ScpDevice
    {
        #region Private fields

        private const int BusWidth = 4;
        private readonly List<int> _pluggedInDevices = new List<int>();
        private int _busOffset;
        private DsState _busState = DsState.Disconnected;

        #endregion
        
        #region Public properties

        private static Guid DeviceClassGuid
        {
            get { return Guid.Parse("{F679F562-3164-42CE-A4DB-E7DDBE723909}"); }
        }

        public DsState State
        {
            get { return _busState; }
        }

        #endregion

        #region Ctor

        public BusDevice() : base(DeviceClassGuid)
        {
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
                var items = new Queue<int>();

                lock (_pluggedInDevices)
                {
                    foreach (var serial in _pluggedInDevices) items.Enqueue(serial - _busOffset);
                }

                while (items.Count > 0) Unplug(items.Dequeue());

                // send un-plug-all to clean the bus from devices stuck in error state
                XOutputWrapper.Instance.UnPlugAll();

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
        /// <returns>The translated data as <see cref="XINPUT_GAMEPAD"/> structure.</returns>
        public XINPUT_GAMEPAD Parse(ScpHidReport inputReport)
        {
            var xButton = X360Button.None;
            var output = new XINPUT_GAMEPAD();

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

                    // face buttons
                    output.wButtons = (ushort) xButton;

                    // trigger
                    output.bLeftTrigger = inputReport[Ds3Axis.L2].Value;
                    output.bRightTrigger = inputReport[Ds3Axis.R2].Value;

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneL, 
                        inputReport[Ds3Axis.Lx].Value,
                        inputReport[Ds3Axis.Ly].Value))
                        // Left Stick DeadZone
                    {
                        output.sThumbLX =
                            (short)
                                +DsMath.Scale(inputReport[Ds3Axis.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                        output.sThumbLY =
                            (short)
                                -DsMath.Scale(inputReport[Ds3Axis.Ly].Value, GlobalConfiguration.Instance.FlipLY);
                    }

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneR,
                        inputReport[Ds3Axis.Rx].Value,
                        inputReport[Ds3Axis.Ry].Value))
                        // Right Stick DeadZone
                    {
                        output.sThumbRX =
                            (short)
                                +DsMath.Scale(inputReport[Ds3Axis.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                        output.sThumbRY =
                            (short)
                                -DsMath.Scale(inputReport[Ds3Axis.Ry].Value, GlobalConfiguration.Instance.FlipRY);
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

                    // face buttons
                    output.wButtons = (ushort) xButton;

                    // trigger
                    output.bLeftTrigger = inputReport[Ds4Axis.L2].Value;
                    output.bRightTrigger = inputReport[Ds4Axis.R2].Value;

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneL, 
                        inputReport[Ds4Axis.Lx].Value,
                        inputReport[Ds4Axis.Ly].Value))
                        // Left Stick DeadZone
                    {
                        output.sThumbLX =
                            (short)
                                +DsMath.Scale(inputReport[Ds4Axis.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                        output.sThumbLY =
                            (short)
                                -DsMath.Scale(inputReport[Ds4Axis.Ly].Value, GlobalConfiguration.Instance.FlipLY);
                    }

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneR, 
                        inputReport[Ds4Axis.Rx].Value,
                        inputReport[Ds4Axis.Ry].Value))
                        // Right Stick DeadZone
                    {
                        output.sThumbRX =
                            (short)
                                +DsMath.Scale(inputReport[Ds4Axis.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                        output.sThumbRY =
                            (short)
                                -DsMath.Scale(inputReport[Ds4Axis.Ry].Value, GlobalConfiguration.Instance.FlipRY);
                    }
                }
                    break;
            }

            return output;
        }

        public bool Plugin(int serial)
        {
            if (GlobalConfiguration.Instance.IsVBusDisabled) return true;

            var retVal = false;

            if (serial < 1 || serial > BusWidth) return false;

            serial += _busOffset;

            if (State != DsState.Connected) return false;

            lock (_pluggedInDevices)
            {
                if (!_pluggedInDevices.Contains(serial))
                {
                    if (XOutputWrapper.Instance.PlugIn(serial - 1))
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
                    if (XOutputWrapper.Instance.UnPlug(serial - 1))
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

        #endregion
    }
}
