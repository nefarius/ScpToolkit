using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Sound;
using ScpControl.Utilities;

namespace ScpControl.Usb.Gamepads
{
    public class UsbGenericGamepad : UsbDevice
    {
        #region Private fields

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private HidDevice _currentHidDevice;
        private HidStream _currentHidStream;

        #endregion

        #region Ctor

        protected UsbGenericGamepad()
        {
            // TODO: Generic isn't currently supported by bus driver
            Model = DsModel.DS3;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     GUID_DEVINTERFACE_HID
        /// </summary>
        public static Guid DeviceClassGuid
        {
            // https://msdn.microsoft.com/en-us/library/windows/hardware/ff545860(v=vs.85).aspx
            get { return Guid.Parse("{4D1E55B2-F16F-11CF-88CB-001111000030}"); }
        }

        public static IList<HidDevice> LocalHidDevices
        {
            get { return new HidDeviceLoader().GetDevices().ToList(); }
        }

        #endregion

        #region Object factory

        public static UsbDevice DeviceFactory(string devicePath)
        {
            short vid, pid;

            GetHardwareId(devicePath, out vid, out pid);

            #region DragonRise Inc. Usb Gamepad SNES

            if (vid == 0x0079 && pid == 0x0011)
            {
                Log.InfoFormat(
                    "DragonRise Inc. Usb Gamepad SNES detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);

                return new UsbSnesGamepad();
            }

            #endregion

            #region LSI Logic Gamepad

            if (vid == 0x0079 && pid == 0x0006)
            {
                Log.InfoFormat(
                    "LSI Logic Gamepad detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);

                return new UsbLsiLogicGamepad();
            }

            #endregion

            #region ShanWan Wireless Gamepad

            if (vid == 0x2563 && pid == 0x0523)
            {
                Log.InfoFormat(
                    "ShanWan Wireless Gamepad detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);

                return new UsbShanWanWirelessGamepad();
            }

            #endregion

            #region GameStop PC Advanced Controller

            if (vid == 0x11FF && pid == 0x3331)
            {
                Log.InfoFormat(
                    "GameStop PC Advanced Controller detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);

                return new UsbGameStopPcAdvanced();
            }

            #endregion

            return null;
        }

        #endregion

        #region Actions

        public override bool Open(string devicePath)
        {
            var loader = new HidDeviceLoader();

            // search for HID
            _currentHidDevice = loader.GetDevices(VendorId, ProductId).FirstOrDefault();

            if (_currentHidDevice == null)
            {
                Log.ErrorFormat("Couldn't find device with VID: {0}, PID: {1}",
                    VendorId, ProductId);
                return false;
            }

            // open HID
            if (!_currentHidDevice.TryOpen(out _currentHidStream))
            {
                Log.ErrorFormat("Couldn't open device {0}", _currentHidDevice);
                return false;
            }

            // since these devices have no MAC address, generate one
            m_Mac = MacAddressGenerator.NewMacAddress;
            DeviceMac = PhysicalAddress.Parse(m_Mac);

            IsActive = true;
            Path = devicePath;

            return IsActive;
        }

        public override bool Start()
        {
            // TODO: remove duplicate code
            if (!IsActive) return State == DsState.Connected;

            State = DsState.Connected;
            PacketCounter = 0;

            Task.Factory.StartNew(MainHidInputReader, _cancellationTokenSource.Token);

            // connection sound
            if (GlobalConfiguration.Instance.IsUsbConnectSoundEnabled)
                AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.UsbConnectSoundFile);

            return State == DsState.Connected;
        }


        public override bool Stop()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            // pad reservation not supported
            State = DsState.Disconnected;

            return true;
        }

        #endregion

        #region Main worker

        private void MainHidInputReader(object o)
        {
            var token = (CancellationToken) o;
            var buffer = new byte[_currentHidDevice.MaxInputReportLength];

            while (!token.IsCancellationRequested)
            {
                var count = _currentHidStream.Read(buffer, 0, buffer.Length);

                if (count > 0)
                {
                    ParseHidReport(buffer);
                }
            }
        }

        #endregion

        #region Unused methods

        protected override void Process(DateTime now)
        {
            // ignore
        }

        public override bool Pair(PhysicalAddress master)
        {
            return false; // ignore
        }

        public override bool Rumble(byte large, byte small)
        {
            return false; // ignore
        }

        #endregion
    }
}
