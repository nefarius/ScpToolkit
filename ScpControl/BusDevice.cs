using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using HidReport.Contract.Enums;
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
        public XINPUT_GAMEPAD Parse(ScpHidReport inputReports)
        {
            var xButton = X360Button.None;
            var output = new XINPUT_GAMEPAD();
            var inputReport = inputReports.HidReport;
            switch (inputReports.Model)
            {
                case DsModel.DS3:
                {
                    // select & start
                    if (inputReport[ButtonsEnum.Select].IsPressed) xButton |= X360Button.Back;
                    if (inputReport[ButtonsEnum.Start].IsPressed) xButton |= X360Button.Start;

                    // d-pad
                    if (inputReport[ButtonsEnum.Up].IsPressed) xButton |= X360Button.Up;
                    if (inputReport[ButtonsEnum.Right].IsPressed) xButton |= X360Button.Right;
                    if (inputReport[ButtonsEnum.Down].IsPressed) xButton |= X360Button.Down;
                    if (inputReport[ButtonsEnum.Left].IsPressed) xButton |= X360Button.Left;

                    // shoulders
                    if (inputReport[ButtonsEnum.L1].IsPressed) xButton |= X360Button.LB;
                    if (inputReport[ButtonsEnum.R1].IsPressed) xButton |= X360Button.RB;

                    // face buttons
                    if (inputReport[ButtonsEnum.Triangle].IsPressed) xButton |= X360Button.Y;
                    if (inputReport[ButtonsEnum.Circle].IsPressed) xButton |= X360Button.B;
                    if (inputReport[ButtonsEnum.Cross].IsPressed) xButton |= X360Button.A;
                    if (inputReport[ButtonsEnum.Square].IsPressed) xButton |= X360Button.X;

                    // PS/Guide
                    if (inputReport[ButtonsEnum.Ps].IsPressed) xButton |= X360Button.Guide;

                    // thumbs
                    if (inputReport[ButtonsEnum.L3].IsPressed) xButton |= X360Button.LS;
                    if (inputReport[ButtonsEnum.R3].IsPressed) xButton |= X360Button.RS;

                    // face buttons
                    output.wButtons = (ushort) xButton;

                    // trigger
                    output.bLeftTrigger = inputReport[AxesEnum.L2].Value;
                    output.bRightTrigger = inputReport[AxesEnum.R2].Value;

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneL, 
                        inputReport[AxesEnum.Lx].Value,
                        inputReport[AxesEnum.Ly].Value))
                        // Left Stick DeadZone
                    {
                        output.sThumbLX =
                            (short)
                                +DsMath.Scale(inputReport[AxesEnum.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                        output.sThumbLY =
                            (short)
                                -DsMath.Scale(inputReport[AxesEnum.Ly].Value, GlobalConfiguration.Instance.FlipLY);
                    }

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneR,
                        inputReport[AxesEnum.Rx].Value,
                        inputReport[AxesEnum.Ry].Value))
                        // Right Stick DeadZone
                    {
                        output.sThumbRX =
                            (short)
                                +DsMath.Scale(inputReport[AxesEnum.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                        output.sThumbRY =
                            (short)
                                -DsMath.Scale(inputReport[AxesEnum.Ry].Value, GlobalConfiguration.Instance.FlipRY);
                    }
                }
                    break;

                case DsModel.DS4:
                {
                    if (inputReport[ButtonsEnum.Share].IsPressed) xButton |= X360Button.Back;
                    if (inputReport[ButtonsEnum.Options].IsPressed) xButton |= X360Button.Start;

                    if (inputReport[ButtonsEnum.Up].IsPressed) xButton |= X360Button.Up;
                    if (inputReport[ButtonsEnum.Right].IsPressed) xButton |= X360Button.Right;
                    if (inputReport[ButtonsEnum.Down].IsPressed) xButton |= X360Button.Down;
                    if (inputReport[ButtonsEnum.Left].IsPressed) xButton |= X360Button.Left;

                    if (inputReport[ButtonsEnum.L1].IsPressed) xButton |= X360Button.LB;
                    if (inputReport[ButtonsEnum.R1].IsPressed) xButton |= X360Button.RB;

                    if (inputReport[ButtonsEnum.Triangle].IsPressed) xButton |= X360Button.Y;
                    if (inputReport[ButtonsEnum.Circle].IsPressed) xButton |= X360Button.B;
                    if (inputReport[ButtonsEnum.Cross].IsPressed) xButton |= X360Button.A;
                    if (inputReport[ButtonsEnum.Square].IsPressed) xButton |= X360Button.X;

                    if (inputReport[ButtonsEnum.Ps].IsPressed) xButton |= X360Button.Guide;

                    if (inputReport[ButtonsEnum.L3].IsPressed) xButton |= X360Button.LS;
                    if (inputReport[ButtonsEnum.R3].IsPressed) xButton |= X360Button.RS;

                    // face buttons
                    output.wButtons = (ushort) xButton;

                    // trigger
                    output.bLeftTrigger = inputReport[AxesEnum.L2].Value;
                    output.bRightTrigger = inputReport[AxesEnum.R2].Value;

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneL, 
                        inputReport[AxesEnum.Lx].Value,
                        inputReport[AxesEnum.Ly].Value))
                        // Left Stick DeadZone
                    {
                        output.sThumbLX =
                            (short)
                                +DsMath.Scale(inputReport[AxesEnum.Lx].Value, GlobalConfiguration.Instance.FlipLX);
                        output.sThumbLY =
                            (short)
                                -DsMath.Scale(inputReport[AxesEnum.Ly].Value, GlobalConfiguration.Instance.FlipLY);
                    }

                    if (!DsMath.DeadZone(GlobalConfiguration.Instance.DeadZoneR, 
                        inputReport[AxesEnum.Rx].Value,
                        inputReport[AxesEnum.Ry].Value))
                        // Right Stick DeadZone
                    {
                        output.sThumbRX =
                            (short)
                                +DsMath.Scale(inputReport[AxesEnum.Rx].Value, GlobalConfiguration.Instance.FlipRX);
                        output.sThumbRY =
                            (short)
                                -DsMath.Scale(inputReport[AxesEnum.Ry].Value, GlobalConfiguration.Instance.FlipRY);
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
