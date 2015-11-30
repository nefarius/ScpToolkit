using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MadMilkman.Ini;

namespace ScpXInputBridge
{
    public partial class XInputDll : IDisposable
    {
        #region Private delegates

        private static readonly Lazy<XInputEnableFunction> OriginalXInputEnableFunction =
            new Lazy<XInputEnableFunction>(() =>
            {
                Initialize();
                return (XInputEnableFunction) GetMethod<XInputEnableFunction>(_dll, "XInputEnable");
            });

        private static readonly Lazy<XInputGetStateFunction> OriginalXInputGetStateFunction = new Lazy
            <XInputGetStateFunction>(
            () =>
            {
                Initialize();
                return (XInputGetStateFunction) GetMethod<XInputGetStateFunction>(_dll, "XInputGetState");
            });

        private static readonly Lazy<XInputSetStateFunction> OriginalXInputSetStateFunction = new Lazy
            <XInputSetStateFunction>(
            () =>
            {
                Initialize();
                return (XInputSetStateFunction) GetMethod<XInputSetStateFunction>(_dll, "XInputSetState");
            });

        private static readonly Lazy<XInputGetCapabilitiesFunction> OriginalXInputGetCapabilitiesFunction = new Lazy
            <XInputGetCapabilitiesFunction>(
            () =>
            {
                Initialize();
                return
                    (XInputGetCapabilitiesFunction)
                        GetMethod<XInputGetCapabilitiesFunction>(_dll, "XInputGetCapabilities");
            });

        private static readonly Lazy<XInputGetDSoundAudioDeviceGuidsFunction>
            OriginalXInputGetDSoundAudioDeviceGuidsFunction = new Lazy<XInputGetDSoundAudioDeviceGuidsFunction>(
                () =>
                {
                    Initialize();
                    return
                        (XInputGetDSoundAudioDeviceGuidsFunction)
                            GetMethod<XInputGetDSoundAudioDeviceGuidsFunction>(_dll,
                                "XInputGetDSoundAudioDeviceGuids");
                });

        private static readonly Lazy<XInputGetBatteryInformationFunction> OriginalXInputGetBatteryInformationFunction = new Lazy
            <XInputGetBatteryInformationFunction>(
            () =>
            {
                Initialize();
                return (XInputGetBatteryInformationFunction)
                    GetMethod<XInputGetBatteryInformationFunction>(_dll, "XInputGetBatteryInformation");
            });

        private static readonly Lazy<XInputGetKeystrokeFunction> OriginalXInputGetKeystrokeFunction = new Lazy
            <XInputGetKeystrokeFunction>(
            () =>
            {
                Initialize();
                return (XInputGetKeystrokeFunction) GetMethod<XInputGetKeystrokeFunction>(_dll, "XInputGetKeystroke");
            });

        #endregion

        #region Methods

        /// <summary>
        ///     Free resources.
        /// </summary>
        /// TODO: does this even get called?
        public void Dispose()
        {
            if (_dll == IntPtr.Zero) return;

            Kernel32Natives.FreeLibrary(_dll);
            _isInitialized = false;
        }

        /// <summary>
        ///     Translates a native method into a managed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the target delegate.</typeparam>
        /// <param name="module">The module name to search the function in.</param>
        /// <param name="methodName">The native finctions' name.</param>
        /// <returns>Returns the managed delegate.</returns>
        private static Delegate GetMethod<T>(IntPtr module, string methodName)
        {
            return Marshal.GetDelegateForFunctionPointer(Kernel32Natives.GetProcAddress(module, methodName), typeof (T));
        }

        /// <summary>
        ///     Loads native dependencies.
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized)
                return;

            #region Prepare logger

            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var logFile = new FileAppender
            {
                AppendToFile = true,
                File = string.Format("XInput1_3_{0}.log.xml", Environment.UserName),
                Layout = new XmlLayoutSchemaLog4j(true)
            };
            logFile.ActivateOptions();
            hierarchy.Root.AddAppender(logFile);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;

            #endregion

            Log.InfoFormat("Library loaded by process {0} [{1}]", 
                Process.GetCurrentProcess().ProcessName,
                Process.GetCurrentProcess().MainWindowTitle);

            Log.Info("Initializing library");
            
            var iniOpts = new IniOptions
            {
                CommentStarter = IniCommentStarter.Semicolon
            };

            var ini = new IniFile(iniOpts);
            var fullPath = Path.Combine(WorkingDirectory, CfgFile);
            Log.DebugFormat("INI-File path: {0}", fullPath);

            if (!File.Exists(fullPath))
            {
                Log.FatalFormat("Configuration file {0} not found", fullPath);
                return;
            }

            try
            {
                // parse data from INI
                ini.Load(fullPath);

                var basePath = ini.Sections["ScpControl"].Keys["BinPath"].Value;
                Log.DebugFormat("ScpToolkit bin path: {0}", basePath);
                var binName = ini.Sections["ScpControl"].Keys["BinName"].Value;
                Log.DebugFormat("ScpControl bin path: {0}", binName);

                // load all assembly dependencies
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var asmName = new AssemblyName(args.Name).Name;
                    Log.DebugFormat("Loading assembly: {0}", asmName);

                    return Assembly.LoadFrom(Path.Combine(basePath, string.Format("{0}.dll", asmName)));
                };

                var scpControl = Assembly.LoadFrom(Path.Combine(basePath, binName));
                var scpProxyType = scpControl.GetType("ScpControl.ScpProxy");

                _scpProxy = Activator.CreateInstance(scpProxyType);

                _scpProxy.Start();
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Error during library initialization: {0}", ex);
                return;
            }

            // if no custom path specified by user, use DLL in system32 dir
            var xinput13Path = ini.Sections["xinput1_3"].Keys.Contains("OriginalFilePath")
                ? ini.Sections["xinput1_3"].Keys["OriginalFilePath"].Value
                : Path.Combine(Environment.SystemDirectory, "xinput1_3.dll");
            Log.DebugFormat("Original XInput 1.3 DLL path: {0}", xinput13Path);

            _dll = Kernel32Natives.LoadLibrary(xinput13Path);

            if (_dll != IntPtr.Zero)
                _isInitialized = true;

            Log.Info("Library initialized");
        }

        #endregion

        #region Private fields

        private static IntPtr _dll = IntPtr.Zero;
        private static volatile bool _isInitialized;
        private static dynamic _scpProxy;
        private const string CfgFile = "ScpXInput.ini";
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Delegates for GetProcAddress

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void XInputEnableFunction(bool enable);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetStateFunction(uint dwUserIndex, ref XINPUT_STATE pState);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputSetStateFunction(uint dwUserIndex, ref XINPUT_VIBRATION pVibration);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetCapabilitiesFunction(uint dwUserIndex, uint dwFlags,
            ref XINPUT_CAPABILITIES pCapabilities);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetDSoundAudioDeviceGuidsFunction(uint dwUserIndex, ref Guid pDSoundRenderGuid,
            ref Guid pDSoundCaptureGuid);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetBatteryInformationFunction(uint dwUserIndex, byte devType,
            ref XINPUT_BATTERY_INFORMATION pBatteryInformation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetKeystrokeFunction(
            uint dwUserIndex, uint dwReserved, ref XINPUT_KEYSTROKE pKeystroke);

        #endregion
    }
}