using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl.Driver
{
    /// <summary>
    ///     Abstract singleton helper class to load native libraries matching the current processor architecture.
    /// </summary>
    /// <typeparam name="T">The type of the derived class.</typeparam>
    public abstract class NativeLibraryWrapper<T> where T : class
    {
        private static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateInstanceOfT);
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Object factory.
        /// </summary>
        /// <returns>Returns a new object.</returns>
        private static T CreateInstanceOfT()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }

        /// <summary>
        ///     Singleton instance.
        /// </summary>
        public static T Instance
        {
            get { return LazyInstance.Value; }
        }

        /// <summary>
        ///     Loads a given native library with Windows API function LoadLibrary() considering the process architechture.
        /// </summary>
        /// <param name="name">A short name of the library (appears in logging).</param>
        /// <param name="x86Path">The relative path to the x86-build of the library.</param>
        /// <param name="amd64Path">The relative path to the amd64-build of the library.</param>
        protected void LoadNativeLibrary(string name, string x86Path, string amd64Path)
        {
            Log.DebugFormat("Preparing to load {0}", name);

            try
            {
                // preloading the library matching the current architecture
                if (Environment.Is64BitProcess)
                {
                    Log.DebugFormat("Called from 64-Bit process");

                    var lib64 = Path.Combine(GlobalConfiguration.AppDirectory, amd64Path);
                    Log.DebugFormat("{0} path: {1}", name, lib64);

                    if (Kernel32.LoadLibrary(lib64) == IntPtr.Zero)
                    {
                        Log.FatalFormat("Couldn't load library {0}: {1}", lib64,
                            new Win32Exception(Marshal.GetLastWin32Error()));
                        return;
                    }

                    Log.DebugFormat("Loaded library: {0}", lib64);
                }
                else
                {
                    Log.DebugFormat("Called from 32-Bit process");

                    var lib32 = Path.Combine(GlobalConfiguration.AppDirectory, x86Path);
                    Log.DebugFormat("{0} path: {1}", name, lib32);

                    if (Kernel32.LoadLibrary(lib32) == IntPtr.Zero)
                    {
                        Log.FatalFormat("Couldn't load library {0}: {1}", lib32,
                            new Win32Exception(Marshal.GetLastWin32Error()));
                        return;
                    }

                    Log.DebugFormat("Loaded library: {0}", lib32);
                }
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Couldn't load library {0}: {1}", name, ex);
            }
        }
    }

    /// <summary>
    ///     Utility class to provide native LoadLibrary() function.
    /// <remarks>Must be in it's own static class to avoid TypeLoadException.</remarks>
    /// </summary>
    public static class Kernel32
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string librayName);
    }
}
