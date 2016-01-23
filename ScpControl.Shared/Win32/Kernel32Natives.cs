using System;
using System.Runtime.InteropServices;

namespace ScpControl.Shared.Win32
{
    /// <summary>
    ///     Windows API function imports.
    /// </summary>
    public static class Kernel32Natives
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        ///     Translates a native method into a managed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the target delegate.</typeparam>
        /// <param name="module">The module name to search the function in.</param>
        /// <param name="methodName">The native finctions' name.</param>
        /// <returns>Returns the managed delegate.</returns>
        public static T GetMethod<T>(IntPtr module, string methodName)
        {
            return (T) Convert.ChangeType(
                Marshal.GetDelegateForFunctionPointer(GetProcAddress(module, methodName), typeof (T)),
                typeof (T));
        }
    }
}
