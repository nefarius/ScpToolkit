using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Utilities;

namespace ScpControl.Bluetooth
{
    public partial class BthDongle
    {
        #region Worker Threads

        /// <summary>
        ///     Main task listening for incoming L2CAP data on the bulk pipe.
        /// </summary>
        /// <param name="o">Cancellation token to abort the tasks inner loop.</param>
        private void L2CapWorker(object o)
        {
            var token = (CancellationToken) o;
            var buffer = new byte[512];

            var transfered = 0;

            Log.DebugFormat("-- Bluetooth  : L2CAP_Worker_Thread Starting (IN: {0:X2}, OUT: {1:X2})", BulkIn, BulkOut);

            // poll device buffer until cancellation requested
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // communication channels use the bulk pipe
                    if (!ReadBulkPipe(buffer, buffer.Length, ref transfered) || transfered <= 0) continue;

                    var packet = new L2CapDataPacket(buffer);

                    var connection = GetConnection(packet);

                    if (connection == null && !_connectionPendingEvent.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        Log.WarnFormat("Couldn't get connection handle [{0:X2}, {1:X2}]", buffer[0], buffer[1]);
                        continue;
                    }

                    connection = GetConnection(packet);

                    if (connection == null) continue;

                    if (connection.Model == DsModel.DS4)
                    {
                        ParseBufferDs4(connection, buffer, transfered);
                    }
                    else
                    {
                        ParseBufferDs3(connection, packet);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error in L2CAP_Worker_Thread: {0}", ex);
                }
            }

            Log.Debug("-- Bluetooth  : L2CAP_Worker_Thread Exiting");
        }

        /// <summary>
        ///     Parses an incoming DualShock 4 HID report.
        /// </summary>
        /// <param name="connection">The device handle the input buffer was received for.</param>
        /// <param name="buffer">The HID report in bytes.</param>
        /// <param name="transfered">The transfered bytes count.</param>
        private void ParseBufferDs4(BthDevice connection, byte[] buffer, int transfered)
        {
            byte[] L2_DCID, L2_SCID;

            if (buffer[6] == 0x01 && buffer[7] == 0x00) // Control Channel
            {
                if (Enum.IsDefined(typeof (L2CAP.Code), buffer[8]))
                {
                    var Event = (L2CAP.Code) buffer[8];

                    switch (Event)
                    {
                            #region L2CAP_Command_Reject

                        case L2CAP.Code.L2CAP_Command_Reject:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                            #endregion

                            #region L2CAP_Connection_Request

                        case L2CAP.Code.L2CAP_Connection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] PSM [{2:X2}]", Event, buffer[8], buffer[12]);

                            L2_SCID = new byte[2] {buffer[14], buffer[15]};
                            L2_DCID = connection.SetConnectionType((L2CAP.PSM) buffer[12], L2_SCID);

                            if (L2CAP.PSM.HID_Interrupt == (L2CAP.PSM) buffer[12])
                            {
                                connection.IsStarted = true;
                            }

                            L2CAP_Connection_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID, L2_DCID,
                                L2CAP.ConnectionResponseResult.ConnectionSuccessful);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Connection_Response,
                                (byte) L2CAP.Code.L2CAP_Connection_Response);

                            L2CAP_Configuration_Request(connection.HciHandle.Bytes, _l2CapDataIdentifier++, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Request,
                                (byte) L2CAP.Code.L2CAP_Configuration_Request);
                            break;

                            #endregion

                            #region L2CAP_Connection_Response

                        case L2CAP.Code.L2CAP_Connection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}]", Event, buffer[8], buffer[16]);
                            break;

                            #endregion

                            #region L2CAP_Configuration_Request

                        case L2CAP.Code.L2CAP_Configuration_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            L2_SCID = connection.Get_SCID(buffer[12], buffer[13]);

                            L2CAP_Configuration_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Response,
                                (byte) L2CAP.Code.L2CAP_Configuration_Response);

                            if (connection.IsServiceStarted)
                            {
                                connection.CanStartHid = true;
                            }
                            break;

                            #endregion

                            #region L2CAP_Configuration_Response

                        case L2CAP.Code.L2CAP_Configuration_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                            if (connection.IsStarted)
                            {
                                OnInitialised(connection);
                            }
                            break;

                            #endregion

                            #region L2CAP_Disconnection_Request

                        case L2CAP.Code.L2CAP_Disconnection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] Handle [{2:X2}{3:X2}]", Event, buffer[8], buffer[15],
                                buffer[14]);

                            L2_SCID = new byte[2] {buffer[14], buffer[15]};

                            L2CAP_Disconnection_Response(connection.HciHandle.Bytes, buffer[9], L2_SCID, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Response,
                                (byte) L2CAP.Code.L2CAP_Disconnection_Response);
                            break;

                            #endregion

                            #region L2CAP_Disconnection_Response

                        case L2CAP.Code.L2CAP_Disconnection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                            #endregion

                            #region L2CAP_Echo_Request

                        case L2CAP.Code.L2CAP_Echo_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                            #endregion

                            #region L2CAP_Echo_Response

                        case L2CAP.Code.L2CAP_Echo_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                            #endregion

                            #region L2CAP_Information_Request

                        case L2CAP.Code.L2CAP_Information_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                            #endregion

                            #region L2CAP_Information_Response

                        case L2CAP.Code.L2CAP_Information_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                            break;

                            #endregion
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
        ///     Parses an incoming DualShock 3 HID report.
        /// </summary>
        /// <param name="connection">The device handle the input buffer was received for.</param>
        /// <param name="packet">The L2CAP data packet.</param>
        private void ParseBufferDs3(BthDevice connection, L2CapDataPacket packet)
        {
            byte[] L2_DCID;
            byte[] L2_SCID;

            if (packet.IsControlChannel) // Control Channel
            {
                if (packet.IsValidSignallingCommandCode)
                {
                    var Event = packet.SignallingCommandCode;

                    switch (Event)
                    {
                            #region L2CAP_Command_Reject

                        case L2CAP.Code.L2CAP_Command_Reject:

                            Log.DebugFormat(">> {0}", Event);
                            break;

                            #endregion

                            #region L2CAP_Connection_Request

                        case L2CAP.Code.L2CAP_Connection_Request:

                            Log.DebugFormat(">> {0} with PSM [{1}] [CID: {2}]", Event,
                                packet.ProtocolServiceMultiplexer,
                                packet.ChannelId);

                            L2_SCID = packet.SourceChannelIdentifier;

                            // set HID command channel for current connection
                            L2_DCID = connection.SetConnectionType(packet.ProtocolServiceMultiplexer, L2_SCID);

                            // send response with connection pending
                            L2CAP_Connection_Response(connection.HciHandle.Bytes,
                                packet.ChannelId, L2_SCID, L2_DCID,
                                L2CAP.ConnectionResponseResult.ConnectionPending,
                                L2CAP.ConnectionResponseStatus.AuthorisationPending);

                            Log.DebugFormat("<< {0} [CID: {1}]", L2CAP.Code.L2CAP_Connection_Response,
                                packet.ChannelId);

                            // send response with connection successful
                            L2CAP_Connection_Response(connection.HciHandle.Bytes,
                                packet.ChannelId, L2_SCID, L2_DCID,
                                L2CAP.ConnectionResponseResult.ConnectionSuccessful);

                            Log.DebugFormat("<< {0} [CID: {1}]", L2CAP.Code.L2CAP_Connection_Response,
                                packet.ChannelId);

                            // send configuration request
                            L2CAP_Configuration_Request(connection.HciHandle.Bytes, _l2CapDataIdentifier++, L2_SCID);

                            Log.DebugFormat("<< {0} [CID: {1}]", L2CAP.Code.L2CAP_Configuration_Request,
                                _l2CapDataIdentifier - 1);
                            break;

                            #endregion

                            #region L2CAP_Connection_Response

                        case L2CAP.Code.L2CAP_Connection_Response:

                            Log.DebugFormat(">> {0} [Result: {1}] [CID: {2}]", Event, packet.Result, packet.ChannelId);

                            var result = packet.Result;

                            L2_SCID = packet.SourceChannelIdentifier;
                            Log.DebugFormat("-- L2_SCID = [{0:X2}, {1:X2}]", L2_SCID[0], L2_SCID[1]);

                            L2_DCID = packet.DestinationChannelIdentifier;
                            Log.DebugFormat("-- L2_DCID = [{0:X2}, {1:X2}]", L2_DCID[0], L2_DCID[1]);

                            // interpret result
                            switch ((L2CAP.ConnectionResponseResult) result)
                            {
                                case L2CAP.ConnectionResponseResult.ConnectionSuccessful:

                                    // destination channel identifier
                                    var DCID = packet.DestinationChannelIdentifierUInt16;
                                    Log.DebugFormat("-- DCID (shifted) = {0:X4}", DCID);

                                    // set HID service channel for current connection
                                    connection.SetConnectionType(L2CAP.PSM.HID_Service, L2_SCID[0], L2_SCID[1], DCID);

                                    // send configuration request
                                    L2CAP_Configuration_Request(connection.HciHandle.Bytes, _l2CapDataIdentifier++,
                                        L2_SCID);

                                    Log.DebugFormat("<< {0} [CID: {1}]",
                                        L2CAP.Code.L2CAP_Configuration_Request,
                                        packet.ChannelId);
                                    break;
                                case L2CAP.ConnectionResponseResult.ConnectionPending:

                                    Log.DebugFormat("-- Connection pending for pad {0}",
                                        connection.PadId.ToString().ToLower());
                                    break;
                                case L2CAP.ConnectionResponseResult.ConnectionRefusedPsmNotNupported:

                                    Log.ErrorFormat(
                                        "Requested Protocol Service Multiplexer not supported on device {0}",
                                        connection.HostAddress);
                                    break;
                                case L2CAP.ConnectionResponseResult.ConnectionRefusedSecurityBlock:

                                    Log.ErrorFormat("Connection refused for security reasons on device {0}",
                                        connection.HostAddress);
                                    break;
                                case L2CAP.ConnectionResponseResult.ConnectionRefusedNoResourcesAvailable:

                                    Log.ErrorFormat("Connection failed for device {0}: no resources available",
                                        connection.HostAddress);
                                    break;

                                default:
                                    Log.WarnFormat("Unknown result: {0}", result);
                                    break;
                            }

                            break;

                            #endregion

                            #region L2CAP_Configuration_Request

                        case L2CAP.Code.L2CAP_Configuration_Request:

                            Log.DebugFormat(">> {0} [CID: {1}]", Event, packet.ChannelId);

                            L2_SCID = connection.Get_SCID(packet.SourceChannelIdentifier);

                            L2CAP_Configuration_Response(connection.HciHandle.Bytes,
                                packet.ChannelId, L2_SCID);
                            Log.DebugFormat("<< {0} [CID: {1}]", L2CAP.Code.L2CAP_Configuration_Response,
                                packet.ChannelId);

                            if (connection.IsServiceStarted)
                            {
                                connection.CanStartHid = true;
                                connection.InitHidReport(packet.RawBytes);
                            }
                            break;

                            #endregion

                            #region L2CAP_Configuration_Response

                        case L2CAP.Code.L2CAP_Configuration_Response:

                            Log.DebugFormat(">> {0} [CID: {1}]", Event, packet.ChannelId);

                            Log.DebugFormat("-- MTU = {0}", packet.MaximumTransmissionUnit);

                            if (connection.CanStartService)
                            {
                                L2_DCID = L2CapDataPacket.UInt16ToBytes(BthConnection.Dcid++);

                                if (!connection.IsFake)
                                {
                                    L2CAP_Connection_Request(connection.HciHandle.Bytes, _l2CapDataIdentifier++, L2_DCID,
                                        L2CAP.PSM.HID_Service);
                                    Log.DebugFormat("<< {0} with PSM [{1}] [CID: {2}]",
                                        L2CAP.Code.L2CAP_Connection_Request,
                                        L2CAP.PSM.HID_Service,
                                        _l2CapDataIdentifier - 1);
                                }
                                else
                                {
                                    connection.SetConnectionType(L2CAP.PSM.HID_Service, L2_DCID);
                                    connection.CanStartService = false;
                                    OnInitialised(connection);
                                }
                            }
                            break;

                            #endregion

                            #region L2CAP_Disconnection_Request

                        case L2CAP.Code.L2CAP_Disconnection_Request:

                            Log.DebugFormat(">> {0} Handle [{1}]", Event, packet.SourceChannelIdentifier);

                            L2_SCID = packet.SourceChannelIdentifier;

                            L2CAP_Disconnection_Response(connection.HciHandle.Bytes,
                                packet.ChannelId, L2_SCID, L2_SCID);

                            Log.DebugFormat("<< {0}", L2CAP.Code.L2CAP_Disconnection_Response);
                            break;

                            #endregion

                            #region L2CAP_Disconnection_Response

                        case L2CAP.Code.L2CAP_Disconnection_Response:

                            Log.DebugFormat(">> {0}", Event);

                            if (connection.CanStartHid)
                            {
                                connection.IsServiceStarted = false;
                                OnInitialised(connection);
                            }
                            break;

                            #endregion

                            #region L2CAP_Echo_Request

                        case L2CAP.Code.L2CAP_Echo_Request:

                            Log.DebugFormat(">> {0}", Event);
                            break;

                            #endregion

                            #region L2CAP_Echo_Response

                        case L2CAP.Code.L2CAP_Echo_Response:

                            Log.DebugFormat(">> {0}", Event);
                            break;

                            #endregion

                            #region L2CAP_Information_Request

                        case L2CAP.Code.L2CAP_Information_Request:

                            Log.DebugFormat(">> {0}", Event);
                            break;

                            #endregion

                            #region L2CAP_Information_Response

                        case L2CAP.Code.L2CAP_Information_Response:

                            Log.DebugFormat(">> {0}", Event);
                            break;

                            #endregion
                    }
                }
            }
            else if (packet.IsHidInputReport)
            {
                // HID report received, parse content and extract gamepad data
                connection.ParseHidReport(packet.RawBytes);
            }
            else if (connection.InitHidReport(packet.RawBytes))
            {
                connection.CanStartHid = true;

                L2_DCID = connection.Get_DCID(L2CAP.PSM.HID_Service);
                L2_SCID = connection.Get_SCID(L2CAP.PSM.HID_Service);

                L2CAP_Disconnection_Request(connection.HciHandle.Bytes, _l2CapDataIdentifier++, L2_SCID, L2_DCID);

                Log.DebugFormat("<< {0}", L2CAP.Code.L2CAP_Disconnection_Request);
            }
        }

        /// <summary>
        ///     Processes communication with the Bluetooth host device.
        /// </summary>
        /// <param name="o">The cancellation token to request task abortion.</param>
        private void HicWorker(object o)
        {
            var token = (CancellationToken) o;
            var nameList = new SortedDictionary<string, string>();
            var hci = IniConfig.Instance.Hci;

            var bStarted = false;
            var bd = string.Empty;

            var buffer = new byte[512];
            var bdAddr = new byte[6];
            var bdLink = new byte[16];
            var bdHandle = new byte[2];

            var transfered = 0;
            var command = HCI.Command.HCI_Null;
            var connection = new BthConnection();

            Log.DebugFormat("Bluetooth Host Controller Interface Task starting on Interrupt Input Pipe: {0:X2}", IntIn);

            HCI_Reset();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // HCI traffic using the interrupt pipe
                    if (ReadIntPipe(buffer, buffer.Length, ref transfered) && transfered > 0)
                    {
                        if (Enum.IsDefined(typeof (HCI.Event), buffer[0]))
                        {
                            var Event = (HCI.Event) buffer[0];

                            switch (Event)
                            {
                                case HCI.Event.HCI_Command_Complete_EV:

                                    command = (HCI.Command) (ushort) (buffer[3] | buffer[4] << 8);
                                    Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}] [{3}]", Event, buffer[0], buffer[5],
                                        command);
                                    break;

                                case HCI.Event.HCI_Command_Status_EV:

                                    command = (HCI.Command) (ushort) (buffer[4] | buffer[5] << 8);
                                    Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}] [{3}]", Event, buffer[0], buffer[2],
                                        command);

                                    if (buffer[2] != 0)
                                    {
                                        switch (command)
                                        {
                                            case HCI.Command.HCI_Write_Simple_Pairing_Mode:
                                            case HCI.Command.HCI_Write_Authentication_Enable:
                                            case HCI.Command.HCI_Set_Event_Mask:

                                                GlobalConfiguration.Instance.DisableSSP = true;
                                                Log.Warn(
                                                    "-- Simple Pairing not supported on this device. [SSP Disabled]");
                                                transfered = HCI_Write_Scan_Enable();
                                                break;
                                        }
                                    }
                                    break;

                                case HCI.Event.HCI_Number_Of_Completed_Packets_EV:
                                    break;

                                default:
                                    Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[0]);
                                    break;
                            }

                            switch (Event)
                            {
                                    #region HCI_Command_Complete_EV

                                case HCI.Event.HCI_Command_Complete_EV:

                                    if (command == HCI.Command.HCI_Reset && buffer[5] == 0 && !bStarted)
                                    {
                                        bStarted = true;
                                        // TODO: do we really need this?
                                        Thread.Sleep(250);

                                        transfered = HCI_Read_BD_Addr();
                                    }

                                    if (command == HCI.Command.HCI_Read_BD_ADDR && buffer[5] == 0)
                                    {
                                        BluetoothHostAddress =
                                            new PhysicalAddress(new[]
                                            {buffer[11], buffer[10], buffer[9], buffer[8], buffer[7], buffer[6]});

                                        transfered = HCI_Read_Buffer_Size();
                                    }

                                    if (command == HCI.Command.HCI_Read_Buffer_Size && buffer[5] == 0)
                                    {
                                        Log.DebugFormat("-- {0:X2}{1:X2}, {2:X2}, {3:X2}{4:X2}, {5:X2}{6:X2}", buffer[7],
                                            buffer[6], buffer[8], buffer[10], buffer[9], buffer[12], buffer[11]);

                                        transfered = HCI_Read_Local_Version_Info();
                                    }

                                    #region Host version

                                    // incoming HCI firmware version information
                                    if (command == HCI.Command.HCI_Read_Local_Version_Info && buffer[5] == 0)
                                    {
                                        var hciMajor = buffer[6];
                                        var lmpMajor = buffer[9];

                                        HciVersion = string.Format("{0}.{1:X4}", buffer[6], buffer[8] << 8 | buffer[7]);
                                        LmpVersion = string.Format("{0}.{1:X4}", buffer[9],
                                            buffer[13] << 8 | buffer[12]);

                                        Log.InfoFormat(
                                            "Initializing Bluetooth host {0} (HCI-Version: {1}, LMP-Version: {2})",
                                            BluetoothHostAddress.AsFriendlyName(),
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
                                            Log.InfoFormat(
                                                "Bluetooth host supports communication with DualShock 3 controllers");
                                        }

                                        // Bluetooth v2.1 + EDR
                                        if (hciMajor >= 4 && lmpMajor >= 4)
                                        {
                                            Log.InfoFormat(
                                                "Bluetooth host supports communication with DualShock 4 controllers");
                                        }

                                        // dongle effectively too old/unsupported 
                                        if (hciMajor < 3 || lmpMajor < 3)
                                        {
                                            Log.FatalFormat(
                                                "Unsupported Bluetooth Specification, aborting communication");
                                            transfered = HCI_Reset();
                                            break;
                                        }

                                        // use simple pairing?
                                        if (GlobalConfiguration.Instance.DisableSSP)
                                        {
                                            transfered = HCI_Write_Scan_Enable();
                                        }
                                        else
                                        {
                                            transfered = HCI_Write_Simple_Pairing_Mode();
                                        }
                                    }

                                    #endregion

                                    if (command == HCI.Command.HCI_Write_Simple_Pairing_Mode)
                                    {
                                        if (buffer[5] == 0)
                                        {
                                            transfered = HCI_Write_Simple_Pairing_Debug_Mode();
                                        }
                                        else
                                        {
                                            GlobalConfiguration.Instance.DisableSSP = true;
                                            Log.Warn("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (command == HCI.Command.HCI_Write_Simple_Pairing_Debug_Mode)
                                    {
                                        transfered = HCI_Write_Authentication_Enable();
                                    }

                                    if (command == HCI.Command.HCI_Write_Authentication_Enable)
                                    {
                                        if (buffer[5] == 0)
                                        {
                                            transfered = HCI_Set_Event_Mask();
                                        }
                                        else
                                        {
                                            GlobalConfiguration.Instance.DisableSSP = true;
                                            Log.Warn("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (command == HCI.Command.HCI_Set_Event_Mask)
                                    {
                                        if (buffer[5] == 0)
                                        {
                                            transfered = HCI_Write_Page_Timeout();
                                        }
                                        else
                                        {
                                            GlobalConfiguration.Instance.DisableSSP = true;
                                            Log.Warn("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (command == HCI.Command.HCI_Write_Page_Timeout && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Page_Scan_Activity();
                                    }

                                    if (command == HCI.Command.HCI_Write_Page_Scan_Activity && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Page_Scan_Type();
                                    }

                                    if (command == HCI.Command.HCI_Write_Page_Scan_Type && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Inquiry_Scan_Activity();
                                    }

                                    if (command == HCI.Command.HCI_Write_Inquiry_Scan_Activity && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Inquiry_Scan_Type();
                                    }

                                    if (command == HCI.Command.HCI_Write_Inquiry_Scan_Type && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Inquiry_Mode();
                                    }

                                    if (command == HCI.Command.HCI_Write_Inquiry_Mode && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Class_of_Device();
                                    }

                                    if (command == HCI.Command.HCI_Write_Class_of_Device && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Extended_Inquiry_Response();
                                    }

                                    if (command == HCI.Command.HCI_Write_Extended_Inquiry_Response && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Local_Name();
                                    }

                                    if (command == HCI.Command.HCI_Write_Local_Name && buffer[5] == 0)
                                    {
                                        transfered = HCI_Write_Scan_Enable();
                                    }

                                    if (command == HCI.Command.HCI_Write_Scan_Enable && buffer[5] == 0)
                                    {
                                        Initialised = true;
                                    }
                                    break;

                                    #endregion

                                    #region HCI_Connection_Request_EV

                                case HCI.Event.HCI_Connection_Request_EV:

                                    Buffer.BlockCopy(buffer, 2, bdAddr, 0, 6);

                                    transfered = HCI_Delete_Stored_Link_Key(bdAddr);
                                    transfered = HCI_Accept_Connection_Request(bdAddr, 0x00);

                                    break;

                                    #endregion

                                    #region HCI_Connection_Complete_EV

                                case HCI.Event.HCI_Connection_Complete_EV:

                                    //buffer[2] contains the status of connection_complete_ev. it's always 0 if succeed
                                    if (buffer[2] == 0x00)
                                    {
                                        Log.DebugFormat("-- HCI_Connection_Complete_EV OK, status: {0:X2}", buffer[2]);

                                        //saving the handle for later usage
                                        bdHandle[0] = buffer[3];
                                        bdHandle[1] = buffer[4];

                                        _connectionPendingEvent.Reset();

                                        //only after connection completed with status 0 we request for controller's name.
                                        transfered = HCI_Remote_Name_Request(bdAddr);
                                    }
                                    else
                                    {
                                        Log.WarnFormat(
                                            "-- HCI_Connection_Complete_EV failed with status: {0:X2}. Connection handle:0x{1:X2}{2:X2}",
                                            buffer[2], buffer[4], buffer[3]);
                                        // TODO: you might want to add some other command here to break or retry.
                                    }

                                    break;

                                    #endregion

                                    #region HCI_Disconnection_Complete_EV

                                case HCI.Event.HCI_Disconnection_Complete_EV:

                                    Remove(buffer[3], (byte) (buffer[4] | 0x20));
                                    break;

                                    #endregion

                                    #region HCI_Number_Of_Completed_Packets_EV

                                case HCI.Event.HCI_Number_Of_Completed_Packets_EV:

                                    for (byte index = 0, ptr = 3; index < buffer[2]; index++, ptr += 4)
                                    {
                                        OnCompletedCount(buffer[ptr], (byte) (buffer[ptr + 1] | 0x20),
                                            (ushort) (buffer[ptr + 2] | buffer[ptr + 3] << 8));
                                    }
                                    break;

                                    #endregion

                                    #region HCI_Remote_Name_Request_Complete_EV

                                case HCI.Event.HCI_Remote_Name_Request_Complete_EV:

                                    bd = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", buffer[8], buffer[7],
                                        buffer[6], buffer[5], buffer[4], buffer[3]);
                                    var nm = new StringBuilder();

                                    // extract product name
                                    for (var index = 9; index < buffer.Length; index++)
                                    {
                                        if (buffer[index] > 0) nm.Append((char) buffer[index]);
                                        else break;
                                    }

                                    var name = nm.ToString();

                                    Log.DebugFormat("-- Remote Name : {0} - {1}", bd, name);

                                    // extract MAC address
                                    Buffer.BlockCopy(buffer, 3, bdAddr, 0, 6);

                                    if (hci.SupportedNames.Any(n => name.StartsWith(n))
                                        || hci.SupportedNames.Any(n => name == n))
                                    {
                                        nameList.Add(bd, name);

                                        // the code below is just cut-paste from "case HCI.Event.HCI_Connection_Complete_EV"
                                        // just some adjustments made in the buffer variables

                                        if (!nameList.Any())
                                            break;

                                        //using there the handles saved earlier
                                        connection = Add(bdHandle[0], (byte) (bdHandle[1] | 0x20), nameList[bd]);

                                        #region Fake DS3 workaround

                                        // skip fake check for version 4 controllers
                                        if (!name.Equals(BthDs4.GenuineProductName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!hci.GenuineMacAddresses.Any(m => bd.StartsWith(m)))
                                            {
                                                connection.IsFake = true;
                                                Log.Warn("Fake DualShock 3 found. Trying Workarounds...");
                                            }
                                            else
                                            {
                                                connection.IsFake = false;
                                                Log.Info("Genuine Sony DualShock 3 found");
                                            }
                                        }
                                        else
                                        {
                                            Log.Info("Sony DualShock 4 found");
                                        }

                                        #endregion

                                        connection.RemoteName = nameList[bd];
                                        nameList.Remove(bd);

                                        connection.DeviceAddress =
                                            new PhysicalAddress(new[]
                                            {buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3]});

                                        _connectionPendingEvent.Set();
                                    }
                                    else
                                    {
                                        transfered = HCI_Reject_Connection_Request(bdAddr, 0x0F);
                                    }
                                    break;

                                    #endregion

                                    #region HCI_Link_Key_Request_EV

                                case HCI.Event.HCI_Link_Key_Request_EV:

                                    Buffer.BlockCopy(buffer, 2, bdAddr, 0, 6);

                                    transfered = HCI_Link_Key_Request_Reply(bdAddr);
                                    transfered = HCI_Set_Connection_Encryption(connection.HciHandle);
                                    break;

                                    #endregion

                                    #region HCI_PIN_Code_Request_EV

                                case HCI.Event.HCI_PIN_Code_Request_EV:

                                    Buffer.BlockCopy(buffer, 2, bdAddr, 0, 6);

                                    transfered = HCI_PIN_Code_Request_Negative_Reply(bdAddr);
                                    break;

                                    #endregion

                                    #region HCI_IO_Capability_Request_EV

                                case HCI.Event.HCI_IO_Capability_Request_EV:

                                    transfered = HCI_IO_Capability_Request_Reply(bdAddr);
                                    break;

                                    #endregion

                                    #region HCI_User_Confirmation_Request_EV

                                case HCI.Event.HCI_User_Confirmation_Request_EV:

                                    transfered = HCI_User_Confirmation_Request_Reply(bdAddr);
                                    break;

                                    #endregion

                                    #region HCI_Link_Key_Notification_EV

                                case HCI.Event.HCI_Link_Key_Notification_EV:

                                    Buffer.BlockCopy(buffer, 2, bdAddr, 0, 6);
                                    Buffer.BlockCopy(buffer, 8, bdLink, 0, 16);

                                    transfered = HCI_Set_Connection_Encryption(connection.HciHandle);
                                    break;

                                    #endregion
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

            Log.Debug("-- Bluetooth  : HCI_Worker_Thread Exiting");
        }

        #endregion
    }
}