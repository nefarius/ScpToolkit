using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ReactiveSockets;
using ScpControl.Shared.Core;

namespace ScpControl.Rx
{
    public class ScpNativeFeedChannel : IChannel<byte[]>
    {
        private readonly IReactiveSocket _socket;
        private BinaryFormatter _binaryFormatter = new BinaryFormatter();
        /// <summary>
        /// Initializes the channel with the given socket, using 
        /// the given encoding for messages.
        /// </summary>
        public ScpNativeFeedChannel(IReactiveSocket socket)
        {
            //Receiver = from packet in socket.Receiver.Buffer(ScpHidReport.Length)
            //           select packet.ToArray();
            this._socket = socket;
            Receiver =
                from header in socket.Receiver.Buffer(4)
                let length = BitConverter.ToInt32(header.ToArray(), 0)
                let body = socket.Receiver.Take(length)
                select body.ToEnumerable().ToArray();
        }

        public IObservable<byte[]> Receiver { get; private set; }

        public Task SendAsync(byte[] message)
        {
            try
            {
                byte[] header = BitConverter.GetBytes(message.Length);
                byte[] payload = header.Concat(message).ToArray();
                return _socket.SendAsync(payload);
            }
            catch (InvalidOperationException)
            {
                return Task.FromResult<object>(null);
            }
        }
    }
}