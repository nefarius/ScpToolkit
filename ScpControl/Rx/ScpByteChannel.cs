using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using ReactiveSockets;

namespace ScpControl.Rx
{
    public enum ScpRequest : byte
    {
        Status = 0x00,
        Rumble = 0x01,
        StatusData = 0x02,
        ConfigRead = 0x03,
        ConfigWrite = 0x04,
        PadPromote = 0x05,
        ProfileList = 0x06,
        SetActiveProfile = 0x07,
        GetXml = 0x08,
        SetXml = 0x09,
        PadDetail = 0x0A,
        NativeFeedAvailable = 0x0B,
        NativeFeed = 0xFF
    }

    public interface IScpPacket<T>
    {
        ScpRequest Request { get; set; }
        T Payload { get; set; }
    }

    public class ScpBytePacket : IScpPacket<byte[]>
    {
        public ScpRequest Request { get; set; }
        public byte[] Payload { get; set; }
    }

    /// <summary>
    /// Implements a communication channel over a socket that 
    /// has a fixed length header and a variable length string 
    /// payload.
    /// </summary>
    public class ScpByteChannel : IChannel<ScpBytePacket>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReactiveSocket _socket;

        /// <summary>
        /// Initializes the channel with the given socket, using 
        /// the given encoding for messages.
        /// </summary>
        public ScpByteChannel(IReactiveSocket socket)
        {
            this._socket = socket;

            Receiver = from header in socket.Receiver.Buffer(sizeof(int))
                       let length = ToInt32(header)
                       let body = socket.Receiver.Take(length).ToEnumerable().ToArray()
                       select new ScpBytePacket() { Request = (ScpRequest)body[0], Payload = body.Skip(1).ToArray() };
        }

        private static int ToInt32(IList<byte> header)
        {
#if DEBUG
            Log.DebugFormat("[{0:D3}] [{1:D3}] [{2:D3}] [{3:D3}]", header[0], header[1], header[2], header[3]);
#endif
            return BitConverter.ToInt32(header.ToArray(), 0);
        }

        public IObservable<ScpBytePacket> Receiver { get; private set; }

        public Task SendAsync(ScpBytePacket message)
        {
            try
            {
                return _socket.SendAsync(Convert(message));
            }
            catch (InvalidOperationException)
            {
                return Task.FromResult(message);
            }
        }

        public Task SendAsync(ScpRequest request, byte[] payload)
        {
            return SendAsync(new ScpBytePacket() { Request = request, Payload = payload });
        }

        public Task SendAsync(ScpRequest request)
        {
            return SendAsync(new ScpBytePacket() { Request = request, Payload = new[] { (byte)0x00 } });
        }

        private static byte[] Convert(ScpBytePacket message)
        {
            var body = message.Payload;
#if DEBUG
            Log.DebugFormat("body.length = {0}", body.Length);
#endif

            var header = BitConverter.GetBytes(body.Length + sizeof(byte)).ToList();
            header.Add((byte)message.Request);
            var payload = header.Concat(body).ToArray();

            return payload;
        }
    }
}
