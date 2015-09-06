using System;
using System.IO;
using System.Text;

namespace ScpControl.Utilities
{
    /// <summary>
    ///     Diagnostics helper class which dumps a byte array to file.
    /// </summary>
    public class DumpHelper
    {
        private StreamWriter _writer;

        public DumpHelper(string file)
        {
            _writer = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                AutoFlush = false
            };
        }

        ~DumpHelper()
        {
            lock (this)
            {
                _writer = null;
            }
        }

        public void DumpArray(byte[] array, int length)
        {
            lock (this)
            {
                if (_writer == null)
                    return;

                var sb = new StringBuilder(length * 2);

                for (int i = 0; i < length; i++)
                {
                    sb.AppendFormat("{0:X2} ", array[i]);
                }

                var line = string.Format("{0} - {1}", DateTime.Now, sb);

                _writer.WriteLine(line);
            }
        }
    }
}
