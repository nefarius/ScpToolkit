// #define HID_REPORT_BENCH
// #define HID_REPORT_BENCH_INC
// #define HID_REPORT_DUMP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ScpControl.Properties;
using ScpControl.ScpCore;
using ScpControl.Utilities;


namespace ScpControl.Bluetooth
{
    public partial class BthDongle
    {
        #region Worker Threads

        private void L2CapWorker(object o)
        {
            var token = (CancellationToken)o;
            var buffer = new byte[512];

            var transfered = 0;

            Log.InfoFormat("-- Bluetooth  : L2CAP_Worker_Thread Starting (IN: {0:X2}, OUT: {1:X2})", BulkIn, BulkOut);

#if HID_REPORT_BENCH
            var sw = new Stopwatch();
            var counter = 0;
            const int samples = 250;
            var values = new List<long>(samples);
            byte rate = 0x01;
#endif

#if HID_REPORT_DUMP

            var dumper = new DumpHelper(System.IO.Path.Combine(WorkingDirectory, string.Format("hid_{0}.dump", Guid.NewGuid())));

#endif

            // poll device buffer until cancellation requested
            while (!token.IsCancellationRequested)
            {
                try
                {
#if HID_REPORT_BENCH
                    sw.Restart();
#endif

                    if (ReadBulkPipe(buffer, buffer.Length, ref transfered) && transfered > 0)
                    {
#if HID_REPORT_BENCH
                        sw.Stop();

                        if (counter++ >= samples)
                        {
                            Log.DebugFormat("[{0:X2}] Average input delay: {1}", rate - 1, values.Average());

                            values.Clear();
                            counter = 0;
                            rate++;
                        }
                        else
                        {
                            values.Add(sw.ElapsedMilliseconds);
                        }
#endif

#if HID_REPORT_DUMP

                        // for diagnostics only; dumps every received report to a file
                        if (Settings.Default.DumpHidReports)
                            dumper.DumpArray(buffer, transfered);

#endif

                        var connection = GetConnection(buffer[0], buffer[1]);

                        if (connection == null)
                            continue;

                        if (connection.Model == DsModel.DS4)
                        {
                            ParseBufferDs4(connection, buffer, transfered);

#if HID_REPORT_BENCH_INC
                            if (counter == samples - 1)
                                (connection as BthDs4).HidReportUpdateRate = rate;
#endif
                        }
                        else
                        {
                            ParseBufferDs3(connection, buffer, transfered);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error in L2CAP_Worker_Thread: {0}", ex);
                }
            }

            Log.Info("-- Bluetooth  : L2CAP_Worker_Thread Exiting");
        }

        /// <summary>
        ///     Parses an incoming DualShock 4 HID report .
        /// </summary>
        /// <param name="connection">The device handle the input buffer was received for.</param>
        /// <param name="buffer">The HID report in bytes.</param>
        /// <param name="transfered">The transfered bytes count.</param>
        private void ParseBufferDs4(BthDevice connection, byte[] buffer, int transfered)
        {
            byte[] L2_DCID, L2_SCID;

            if (buffer[6] == 0x01 && buffer[7] == 0x00) // Control Channel
            {
                if (Enum.IsDefined(typeof(L2CAP.Code), buffer[8]))
                {
                    var Event = (L2CAP.Code)buffer[8];

                    switch (Event)
                    {
                        case L2CAP.Code.L2CAP_Command_Reject:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Connection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] PSM [{2:X2}]", Event, buffer[8], buffer[12]);

                            L2_SCID = new byte[2] { buffer[14], buffer[15] };
                            L2_DCID = connection.SetConnectionType((L2CAP.PSM)buffer[12], L2_SCID);

                            if (L2CAP.PSM.HID_Interrupt == (L2CAP.PSM)buffer[12])
                            {
                                connection.IsStarted = true;
                            }

                            L2CAP_Connection_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID, L2_DCID, 0x00);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Connection_Response,
                                (byte)L2CAP.Code.L2CAP_Connection_Response);

                            L2CAP_Configuration_Request(connection.HciHandle.Bytes, _hidReportId++, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Request,
                                (byte)L2CAP.Code.L2CAP_Configuration_Request);
                            break;

                        case L2CAP.Code.L2CAP_Connection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}]", Event, buffer[8], buffer[16]);
                            break;

                        case L2CAP.Code.L2CAP_Configuration_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            L2_SCID = connection.Get_SCID(buffer[12], buffer[13]);

                            L2CAP_Configuration_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Response,
                                (byte)L2CAP.Code.L2CAP_Configuration_Response);

                            if (connection.IsServiceStarted)
                            {
                                connection.CanStartHid = true;
                            }
                            break;

                        case L2CAP.Code.L2CAP_Configuration_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            if (connection.IsStarted)
                            {
                                OnInitialised(connection);
                            }
                            break;

                        case L2CAP.Code.L2CAP_Disconnection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] Handle [{2:X2}{3:X2}]", Event, buffer[8], buffer[15],
                                buffer[14]);

                            L2_SCID = new byte[2] { buffer[14], buffer[15] };

                            L2CAP_Disconnection_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Response,
                                (byte)L2CAP.Code.L2CAP_Disconnection_Response);
                            break;

                        case L2CAP.Code.L2CAP_Disconnection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Echo_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Echo_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Information_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Information_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;
                    }
                }
            }
            else if (buffer[8] == 0xA1 && buffer[9] == 0x11 && transfered == 87)
            {
                // HID report received, parse content and extract gamepad data
                connection.ParseHidReport(buffer);
            }
            else if (connection.InitHidReport(buffer))
            {
                connection.CanStartHid = true;
            }
        }

        /// <summary>
        ///     Parses an incoming DualShock 3 HID report .
        /// </summary>
        /// <param name="connection">The device handle the input buffer was received for.</param>
        /// <param name="buffer">The HID report in bytes.</param>
        /// <param name="transfered">The transfered bytes count.</param>
        private void ParseBufferDs3(BthDevice connection, byte[] buffer, int transfered)
        {
            byte[] L2_DCID;
            byte[] L2_SCID;

            if (buffer[6] == 0x01 && buffer[7] == 0x00) // Control Channel
            {
                if (Enum.IsDefined(typeof(L2CAP.Code), buffer[8]))
                {
                    var Event = (L2CAP.Code)buffer[8];

                    switch (Event)
                    {
                        case L2CAP.Code.L2CAP_Command_Reject:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Connection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] PSM [{2:X2}]", Event, buffer[8], buffer[12]);

                            L2_SCID = new byte[2] { buffer[14], buffer[15] };
                            L2_DCID = connection.SetConnectionType((L2CAP.PSM)buffer[12], L2_SCID);

                            L2CAP_Connection_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID,
                                L2_DCID, 0x00);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Connection_Response,
                                (byte)L2CAP.Code.L2CAP_Connection_Response);

                            L2CAP_Configuration_Request(connection.HciHandle.Bytes, _hidReportId++, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Request,
                                (byte)L2CAP.Code.L2CAP_Configuration_Request);
                            break;

                        case L2CAP.Code.L2CAP_Connection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}]", Event, buffer[8], buffer[16]);

                            if (buffer[16] == 0) // Success
                            {
                                L2_SCID = new byte[2] { buffer[12], buffer[13] };
                                L2_DCID = new byte[2] { buffer[14], buffer[15] };

                                var DCID = (ushort)(buffer[15] << 8 | buffer[14]);

                                connection.SetConnectionType(L2CAP.PSM.HID_Service, L2_SCID[0], L2_SCID[1], DCID);

                                L2CAP_Configuration_Request(connection.HciHandle.Bytes, _hidReportId++, L2_SCID);
                                Log.DebugFormat("<< {0} [{1:X2}]",
                                    L2CAP.Code.L2CAP_Configuration_Request,
                                    (byte)L2CAP.Code.L2CAP_Configuration_Request);
                            }
                            break;

                        case L2CAP.Code.L2CAP_Configuration_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            L2_SCID = connection.Get_SCID(buffer[12], buffer[13]);

                            L2CAP_Configuration_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Response,
                                (byte)L2CAP.Code.L2CAP_Configuration_Response);

                            if (connection.IsServiceStarted)
                            {
                                connection.CanStartHid = true;
                                connection.InitHidReport(buffer);
                            }
                            break;

                        case L2CAP.Code.L2CAP_Configuration_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            if (connection.CanStartService)
                            {
                                UInt16 DCID = BthConnection.Dcid++;
                                L2_DCID = new Byte[2] { (Byte)((DCID >> 0) & 0xFF), (Byte)((DCID >> 8) & 0xFF) };

                                if (!connection.IsFake)
                                {
                                    L2CAP_Connection_Request(connection.HciHandle.Bytes, _hidReportId++, L2_DCID, L2CAP.PSM.HID_Service);
                                    Log.DebugFormat("<< {0} [{1:X2}] PSM [{2:X2}]",
                                        L2CAP.Code.L2CAP_Connection_Request,
                                        (Byte)L2CAP.Code.L2CAP_Connection_Request,
                                        (Byte)L2CAP.PSM.HID_Service);
                                }
                                else
                                {
                                    connection.CanStartService = false;
                                    OnInitialised(connection);
                                }
                            }
                            break;

                        case L2CAP.Code.L2CAP_Disconnection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] Handle [{2:X2}{3:X2}]", Event, buffer[8],
                                buffer[15], buffer[14]);

                            L2_SCID = new byte[2] { buffer[14], buffer[15] };

                            L2CAP_Disconnection_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID,
                                L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Response,
                                (byte)L2CAP.Code.L2CAP_Disconnection_Response);
                            break;

                        case L2CAP.Code.L2CAP_Disconnection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            if (connection.CanStartHid)
                            {
                                connection.IsServiceStarted = false;
                                OnInitialised(connection);
                            }
                            break;

                        case L2CAP.Code.L2CAP_Echo_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Echo_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Information_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Information_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;
                    }
                }
            }
            else if (buffer[8] == 0xA1 && buffer[9] == 0x01 && transfered == 58)
            {
                // HID report received, parse content and extract gamepad data
                connection.ParseHidReport(buffer);
            }
            else if (connection.InitHidReport(buffer))
            {
                connection.CanStartHid = true;

                L2_DCID = connection.Get_DCID(L2CAP.PSM.HID_Service);
                L2_SCID = connection.Get_SCID(L2CAP.PSM.HID_Service);

                L2CAP_Disconnection_Request(connection.HciHandle.Bytes, _hidReportId++, L2_SCID, L2_DCID);
                Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Request,
                    (byte)L2CAP.Code.L2CAP_Disconnection_Request);
            }
        }

        private void HicWorker(object o)
        {
            var token = (CancellationToken)o;
            var nameList = new SortedDictionary<string, string>();
            var hci = IniConfig.Instance.Hci;

            var bStarted = false;
            var bd = string.Empty;

            var Buffer = new byte[512];
            var BD_Addr = new byte[6];
            var BD_Link = new byte[16];

            var Transfered = 0;
            HCI.Event Event;
            var Command = HCI.Command.HCI_Null;
            var Connection = new BthConnection();

            Log.InfoFormat("-- Bluetooth  : HCI_Worker_Thread Starting (IN: {0:X2})", IntIn);

            HCI_Reset();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (ReadIntPipe(Buffer, Buffer.Length, ref Transfered) && Transfered > 0)
                    {
                        if (Enum.IsDefined(typeof(HCI.Event), Buffer[0]))
                        {
                            Event = (HCI.Event)Buffer[0];

                            switch (Event)
                            {
                                case HCI.Event.HCI_Command_Complete_EV:

                                    Command = (HCI.Command)(ushort)(Buffer[3] | Buffer[4] << 8);
                                    Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}] [{3}]", Event, Buffer[0], Buffer[5],
                                        Command);
                                    break;

                                case HCI.Event.HCI_Command_Status_EV:

                                    Command = (HCI.Command)(ushort)(Buffer[4] | Buffer[5] << 8);
                                    Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}] [{3}]", Event, Buffer[0], Buffer[2],
                                        Command);

                                    if (Buffer[2] != 0)
                                    {
                                        switch (Command)
                                        {
                                            case HCI.Command.HCI_Write_Simple_Pairing_Mode:
                                            case HCI.Command.HCI_Write_Authentication_Enable:
                                            case HCI.Command.HCI_Set_Event_Mask:

                                                GlobalConfiguration.Instance.DisableSSP = true;
                                                Log.Warn(
                                                    "-- Simple Pairing not supported on this device. [SSP Disabled]");
                                                Transfered = HCI_Write_Scan_Enable();
                                                break;
                                        }
                                    }
                                    break;

                                case HCI.Event.HCI_Number_Of_Completed_Packets_EV:
                                    break;

                                default:
                                    Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[0]);
                                    break;
                            }

                            switch (Event)
                            {
                                case HCI.Event.HCI_Command_Complete_EV:

                                    if (Command == HCI.Command.HCI_Reset && Buffer[5] == 0 && !bStarted)
                                    {
                                        bStarted = true;
                                        Thread.Sleep(250);

                                        Transfered = HCI_Read_BD_Addr();
                                    }

                                    if (Command == HCI.Command.HCI_Read_BD_ADDR && Buffer[5] == 0)
                                    {
                                        _localMac = new[] { Buffer[6], Buffer[7], Buffer[8], Buffer[9], Buffer[10], Buffer[11] };

                                        Transfered = HCI_Read_Buffer_Size();
                                    }

                                    if (Command == HCI.Command.HCI_Read_Buffer_Size && Buffer[5] == 0)
                                    {
                                        Log.DebugFormat("-- {0:X2}{1:X2}, {2:X2}, {3:X2}{4:X2}, {5:X2}{6:X2}", Buffer[7],
                                            Buffer[6], Buffer[8], Buffer[10], Buffer[9], Buffer[12], Buffer[11]);

                                        Transfered = HCI_Read_Local_Version_Info();
                                    }

                                    // incoming HCI firmware version information
                                    if (Command == HCI.Command.HCI_Read_Local_Version_Info && Buffer[5] == 0)
                                    {
                                        var hciMajor = Buffer[6];
                                        var lmpMajor = Buffer[9];

                                        HciVersion = string.Format("{0}.{1:X4}", Buffer[6], Buffer[8] << 8 | Buffer[7]);
                                        LmpVersion = string.Format("{0}.{1:X4}", Buffer[9],
                                            Buffer[13] << 8 | Buffer[12]);

                                        Log.InfoFormat("-- Master {0}, HCI_Version {1}, LMP_Version {2}", Local,
                                            HciVersion, LmpVersion);

                                        /* analyzes Host Controller Interface (HCI) major version
                                         * see https://www.bluetooth.org/en-us/specification/assigned-numbers/host-controller-interface
                                         * */
                                        switch (hciMajor)
                                        {
                                            case 0:
                                                Log.DebugFormat("HCI_Version: Bluetooth® Core Specification 1.0b");
                                                break;
                                            case 1:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 1.1");
                                                break;
                                            case 2:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 1.2");
                                                break;
                                            case 3:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 2.0 + EDR");
                                                break;
                                            case 4:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 2.1 + EDR");
                                                break;
                                            case 5:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 3.0 + HS");
                                                break;
                                            case 6:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 4.0");
                                                break;
                                            case 7:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 4.1");
                                                break;
                                            case 8:
                                                Log.DebugFormat("HCI_Version: Bluetooth Core Specification 4.2");
                                                break;
                                            default:
                                                // this should not happen
                                                Log.ErrorFormat("HCI_Version: Specification unknown");
                                                break;
                                        }

                                        /* analyzes Link Manager Protocol (LMP) major version
                                         * see https://www.bluetooth.org/en-us/specification/assigned-numbers/link-manager
                                         * */
                                        switch (lmpMajor)
                                        {
                                            case 0:
                                                Log.DebugFormat("LMP_Version: Bluetooth® Core Specification 1.0b");
                                                break;
                                            case 1:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 1.1");
                                                break;
                                            case 2:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 1.2");
                                                break;
                                            case 3:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 2.0 + EDR");
                                                break;
                                            case 4:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 2.1 + EDR");
                                                break;
                                            case 5:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 3.0 + HS");
                                                break;
                                            case 6:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 4.0");
                                                break;
                                            case 7:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 4.1");
                                                break;
                                            case 8:
                                                Log.DebugFormat("LMP_Version: Bluetooth Core Specification 4.2");
                                                break;
                                            default:
                                                // this should not happen
                                                Log.ErrorFormat("LMP_Version: Specification unknown");
                                                break;
                                        }

                                        // Bluetooth v2.0 + EDR
                                        if (hciMajor >= 3 && lmpMajor >= 3)
                                        {
                                            Log.InfoFormat("Bluetooth host supports communication with DualShock 3 controllers");
                                        }

                                        // Bluetooth v2.1 + EDR
                                        if (hciMajor >= 4 && lmpMajor >= 4)
                                        {
                                            Log.InfoFormat("Bluetooth host supports communication with DualShock 4 controllers");
                                        }

                                        // dongle effectively too old/unsupported 
                                        if (hciMajor < 3 || lmpMajor < 3)
                                        {
                                            Log.FatalFormat("Unsupported Bluetooth Specification, aborting communication");
                                            Transfered = HCI_Reset();
                                            break;
                                        }

                                        // use simple pairing?
                                        if (GlobalConfiguration.Instance.DisableSSP)
                                        {
                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                        else
                                        {
                                            Transfered = HCI_Write_Simple_Pairing_Mode();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Write_Simple_Pairing_Mode)
                                    {
                                        if (Buffer[5] == 0)
                                        {
                                            Transfered = HCI_Write_Simple_Pairing_Debug_Mode();
                                        }
                                        else
                                        {
                                            GlobalConfiguration.Instance.DisableSSP = true;
                                            Log.Warn("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Write_Simple_Pairing_Debug_Mode)
                                    {
                                        Transfered = HCI_Write_Authentication_Enable();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Authentication_Enable)
                                    {
                                        if (Buffer[5] == 0)
                                        {
                                            Transfered = HCI_Set_Event_Mask();
                                        }
                                        else
                                        {
                                            GlobalConfiguration.Instance.DisableSSP = true;
                                            Log.Warn("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Set_Event_Mask)
                                    {
                                        if (Buffer[5] == 0)
                                        {
                                            Transfered = HCI_Write_Page_Timeout();
                                        }
                                        else
                                        {
                                            GlobalConfiguration.Instance.DisableSSP = true;
                                            Log.Warn("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Write_Page_Timeout && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Page_Scan_Activity();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Page_Scan_Activity && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Page_Scan_Type();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Page_Scan_Type && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Inquiry_Scan_Activity();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Inquiry_Scan_Activity && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Inquiry_Scan_Type();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Inquiry_Scan_Type && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Inquiry_Mode();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Inquiry_Mode && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Class_of_Device();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Class_of_Device && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Extended_Inquiry_Response();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Extended_Inquiry_Response && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Local_Name();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Local_Name && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Scan_Enable();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Scan_Enable && Buffer[5] == 0)
                                    {
                                        Initialised = true;
                                    }
                                    break;

                                case HCI.Event.HCI_Connection_Request_EV:

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 2];

                                    Transfered = HCI_Delete_Stored_Link_Key(BD_Addr);
                                    Transfered = HCI_Remote_Name_Request(BD_Addr);
                                    break;

                                case HCI.Event.HCI_Connection_Complete_EV:

                                    bd = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", Buffer[10],
                                        Buffer[9], Buffer[8], Buffer[7], Buffer[6], Buffer[5]);

                                    if (!nameList.Any())
                                        break;

                                    Connection = Add(Buffer[3], (byte)(Buffer[4] | 0x20), nameList[bd]);

                                    #region Fake DS3 workaround

                                    if (!hci.GenuineMacAddresses.Any(m => bd.StartsWith(m)))
                                    {
                                        Connection.IsFake = true;
                                        Log.Debug("-- Fake DualShock 3 found. Workaround applied");
                                    }
                                    else
                                    {
                                        Connection.IsFake = false;
                                        Log.Debug("-- Genuine Sony DualShock 3 found");
                                    }

                                    #endregion

                                    Connection.RemoteName = nameList[bd];
                                    nameList.Remove(bd);
                                    Connection.BdAddress = new[] { Buffer[10], Buffer[9], Buffer[8], Buffer[7], Buffer[6], Buffer[5] };
                                    break;

                                case HCI.Event.HCI_Disconnection_Complete_EV:

                                    Remove(Buffer[3], (byte)(Buffer[4] | 0x20));
                                    break;

                                case HCI.Event.HCI_Number_Of_Completed_Packets_EV:

                                    for (byte Index = 0, Ptr = 3; Index < Buffer[2]; Index++, Ptr += 4)
                                    {
                                        OnCompletedCount(Buffer[Ptr], (byte)(Buffer[Ptr + 1] | 0x20),
                                            (ushort)(Buffer[Ptr + 2] | Buffer[Ptr + 3] << 8));
                                    }
                                    break;

                                case HCI.Event.HCI_Remote_Name_Request_Complete_EV:

                                    bd = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", Buffer[8], Buffer[7],
                                        Buffer[6], Buffer[5], Buffer[4], Buffer[3]);
                                    var nm = new StringBuilder();

                                    for (var Index = 9; Index < Buffer.Length; Index++)
                                    {
                                        if (Buffer[Index] > 0) nm.Append((char)Buffer[Index]);
                                        else break;
                                    }

                                    var Name = nm.ToString();

                                    Log.InfoFormat("-- Remote Name : {0} - {1}", bd, Name);

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 3];

                                    if (hci.SupportedNames.Any(n => Name.StartsWith(n))
                                        || hci.SupportedNames.Any(n => Name == n))
                                    {
                                        nameList.Add(bd, nm.ToString());

                                        Transfered = HCI_Accept_Connection_Request(BD_Addr, 0x00);
                                    }
                                    else
                                    {
                                        Transfered = HCI_Reject_Connection_Request(BD_Addr, 0x0F);
                                    }
                                    break;

                                case HCI.Event.HCI_Link_Key_Request_EV:

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 2];

                                    Transfered = HCI_Link_Key_Request_Reply(BD_Addr);
                                    Transfered = HCI_Set_Connection_Encryption(Connection.HciHandle);
                                    break;

                                case HCI.Event.HCI_PIN_Code_Request_EV:

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 2];

                                    Transfered = HCI_PIN_Code_Request_Negative_Reply(BD_Addr);
                                    break;

                                case HCI.Event.HCI_IO_Capability_Request_EV:

                                    Transfered = HCI_IO_Capability_Request_Reply(BD_Addr);
                                    break;

                                case HCI.Event.HCI_User_Confirmation_Request_EV:

                                    Transfered = HCI_User_Confirmation_Request_Reply(BD_Addr);
                                    break;

                                case HCI.Event.HCI_Link_Key_Notification_EV:

                                    for (var Index = 0; Index < 6; Index++) BD_Addr[Index] = Buffer[Index + 2];
                                    for (var Index = 0; Index < 16; Index++) BD_Link[Index] = Buffer[Index + 8];

                                    Transfered = HCI_Set_Connection_Encryption(Connection.HciHandle);
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error in HCI_Worker_Thread: {0}", ex);
                }
            }

            HCI_Reset();

            Log.Info("-- Bluetooth  : HCI_Worker_Thread Exiting");
        }

        #endregion
    }
}
