using System;
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

            // preloading the library matching the current architecture
            if (Environment.Is64BitProcess)
            {
                Log.DebugFormat("Called from 64-Bit process");

                var lib64 = Path.Combine(GlobalConfiguration.AppDirectory, amd64Path);
                Log.DebugFormat("{0} path: {1}", name, lib64);

                Kernel32.LoadLibrary(lib64);

                Log.DebugFormat("Loaded library: {0}", lib64);
            }
            else
            {
                Log.DebugFormat("Called from 32-Bit process");

                var lib32 = Path.Combine(GlobalConfiguration.AppDirectory, x86Path);
                Log.DebugFormat("{0} path: {1}", name, lib32);

                Kernel32.LoadLibrary(lib32);

                Log.DebugFormat("Loaded library: {0}", lib32);
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
