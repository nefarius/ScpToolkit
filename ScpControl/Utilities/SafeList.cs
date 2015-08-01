using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ScpControl.Utilities
{
    public class SafeList<T> : IEnumerable<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _sync = new object();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (_sync)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_sync)
            {
                return _list.GetEnumerator();
            }
        }

        public void Add(T value)
        {
            lock (_sync)
            {
                _list.Add(value);
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            lock (_sync)
            {
                return _list.RemoveAll(match);
            }
        }

        public bool Contains(T item)
        {
            lock (_sync)
            {
                return _list.Contains(item);
            }
        }

        public T Find(Predicate<T> match)
        {
            lock (_sync)
            {
                return _list.Find(match);
            }
        }

        public List<T>.Enumerator GetEnumerator()
        {
            lock (_sync)
            {
                return _list.GetEnumerator();
            }
        }

        public bool Remove(T item)
        {
            lock (_sync)
            {
                return _list.Remove(item);
            }
        }

        public void Lock()
        {
            Monitor.Enter(_sync);
        }

        public void Unlock()
        {
            Monitor.Exit(_sync);
        }
    }
}