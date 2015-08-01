using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        NativeFeedAvailable = 0x0B
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
    ///     Implements a communication channel over a socket that
    ///     has a fixed length header and a variable length string
    ///     payload.
    /// </summary>
    public class ScpByteChannel : IChannel<ScpBytePacket>
    {
        private readonly IReactiveSocket _socket;

        /// <summary>
        ///     Initializes the channel with the given socket, using
        ///     the given encoding for messages.
        /// </summary>
        public ScpByteChannel(IReactiveSocket socket)
        {
            _socket = socket;

            Receiver = from header in socket.Receiver.Buffer(sizeof (int))
                let length = BitConverter.ToInt32(header.ToArray(), 0)
                let body = socket.Receiver.Take(length).ToEnumerable().ToArray()
                select new ScpBytePacket {Request = (ScpRequest) body[0], Payload = body.Skip(1).ToArray()};
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
            return SendAsync(new ScpBytePacket {Request = request, Payload = payload});
        }

        public Task SendAsync(ScpRequest request)
        {
            return SendAsync(new ScpBytePacket {Request = request});
        }

        private static byte[] Convert(ScpBytePacket message)
        {
            byte[] payload;

            if (message.Payload != null)
            {
                // build buffer with length, request and payload
                var body = message.Payload;
                var header = BitConverter.GetBytes(body.Length + sizeof (byte)).ToList();
                header.Add((byte) message.Request);
                payload = header.Concat(body).ToArray();
            }
            else
            {
                // build buffer with length and request only
                var header = BitConverter.GetBytes(sizeof (byte)).ToList();
                header.Add((byte) message.Request);
                payload = header.ToArray();
            }

            return payload;
        }
    }
}