using System;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
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

    public class ScpCommandPacket : EventArgs, IScpPacket<byte[]>, ICloneable
    {
        public ScpRequest Request { get; set; }
        public byte[] Payload { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public ScpCommandPacket ForwardPacket()
        {
            return (ScpCommandPacket)Clone();
        }
    }

    /// <summary>
    ///     Implements a communication channel over a socket that
    ///     has a fixed length header and a variable length string
    ///     payload.
    /// </summary>
    public class ScpCommandChannel : IChannel<ScpCommandPacket>
    {
        private readonly IReactiveSocket _socket;

        /// <summary>
        ///     Initializes the channel with the given socket, using
        ///     the given encoding for messages.
        /// </summary>
        public ScpCommandChannel(IReactiveSocket socket)
        {
            _socket = socket;

            Receiver = from header in socket.Receiver.Buffer(sizeof(int))
                       let length = BitConverter.ToInt32(header.ToArray(), 0)
                       let body = socket.Receiver.Take(length).ToEnumerable().ToArray()
                       select new ScpCommandPacket { Request = (ScpRequest)body[0], Payload = body.Skip(1).ToArray() };
        }

        public IObservable<ScpCommandPacket> Receiver { get; private set; }

        public Task SendAsync(ScpCommandPacket message)
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
            return SendAsync(new ScpCommandPacket { Request = request, Payload = payload });
        }

        public Task SendAsync(ScpRequest request)
        {
            return SendAsync(new ScpCommandPacket { Request = request });
        }

        private static byte[] Convert(ScpCommandPacket message)
        {
            byte[] payload;

            if (message.Payload != null)
            {
                // build buffer with length, request and payload
                var body = message.Payload;
                var header = BitConverter.GetBytes(body.Length + sizeof(byte)).ToList();
                header.Add((byte)message.Request);
                payload = header.Concat(body).ToArray();
            }
            else
            {
                // build buffer with length and request only
                var header = BitConverter.GetBytes(sizeof(byte)).ToList();
                header.Add((byte)message.Request);
                payload = header.ToArray();
            }

            return payload;
        }
    }
}