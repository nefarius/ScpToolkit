using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveSockets;

namespace ScpControl.Rx
{
    public class ScpNativeFeedChannel : IChannel<byte[]>
    {
        private readonly IReactiveSocket _socket;

        /// <summary>
        /// Initializes the channel with the given socket, using 
        /// the given encoding for messages.
        /// </summary>
        public ScpNativeFeedChannel(IReactiveSocket socket)
        {
            this._socket = socket;

            Receiver = from header in socket.Receiver.Buffer(sizeof(int))
                       let length = BitConverter.ToInt32(header.ToArray(), 0)
                       let body = socket.Receiver.Take(length).ToEnumerable().ToArray()
                       select body;
        }

        public IObservable<byte[]> Receiver { get; private set; }

        public System.Threading.Tasks.Task SendAsync(byte[] message)
        {
            return _socket.SendAsync(Convert(message));
        }

        private static byte[] Convert(byte[] message)
        {
            var body = message;
            var header = BitConverter.GetBytes(body.Length);
            var payload = header.Concat(body).ToArray();

            return payload;
        }
    }
}