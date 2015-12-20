using System;
using System.IO;
using DBreeze;
using ScpControl.ScpCore;

namespace ScpControl.Database
{
    /// <summary>
    ///     Wrapper for embedded object database.
    /// </summary>
    public class ScpDb : IDisposable
    {
        public DBreezeEngine Engine { get; private set; }

        private static string DbPath
        {
            get
            {
                var path = Path.Combine(GlobalConfiguration.AppDirectory, "Db");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public ScpDb()
        {
            Engine = new DBreezeEngine(DbPath);
        }

        ~ScpDb()
        {
            Dispose();
        }

        public static string TableDevices { get { return "tScpDevices"; } }

        public void Dispose()
        {
            if (Engine != null)
                Engine.Dispose();
        }
    }
}
