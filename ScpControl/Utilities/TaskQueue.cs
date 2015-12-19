using System;
using System.Threading.Tasks;

namespace ScpControl.Utilities
{
    /// <summary>
    ///     A non-blocking event handling queue.
    /// </summary>
    /// <remarks>http://stackoverflow.com/a/32993768/490629</remarks>
    public class TaskQueue
    {
        private Task _previous = Task.FromResult(false);
        private readonly object _key = new object();

        public Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            lock (_key)
            {
                var next = _previous.ContinueWith(t => taskGenerator()).Unwrap();
                _previous = next;
                return next;
            }
        }
        public Task Enqueue(Func<Task> taskGenerator)
        {
            lock (_key)
            {
                var next = _previous.ContinueWith(t => taskGenerator()).Unwrap();
                _previous = next;
                return next;
            }
        }
    }
}
