using System;
using System.Threading.Tasks;

namespace ScpControl.Rx
{
    /// <summary>
    /// Interface for a bidirectional communication channel
    /// that exchanges messages of a given type.
    /// </summary>
    public interface IChannel<T>
    {
        /// <summary>
        /// Observable receiver for incoming messages.
        /// </summary>
        IObservable<T> Receiver { get; }

        /// <summary>
        /// Sends asynchronously the given message through the channel.
        /// </summary>
        Task SendAsync(T message);
    }
}
