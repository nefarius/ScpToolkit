using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ScpControl
{
    /// <summary>
    ///     A high precision timer.
    /// </summary>
    public partial class ScpTimer : Component
    {
        #region Private fields

        private readonly WaitOrTimerDelegate _callback;
        private IntPtr _timerHandle = IntPtr.Zero;

        #endregion

        #region Ctors

        public ScpTimer()
        {
            InitializeComponent();

            Interval = 100;
            _callback = OnTick;
        }

        public ScpTimer(IContainer container) : this()
        {
            container.Add(this);
        }

        #endregion

        #region Public properties

        // TODO: what's this used for?!
        public object Tag { get; set; }

        public bool Enabled
        {
            get { return (_timerHandle != IntPtr.Zero); }
            set
            {
                lock (this)
                {
                    if (Enabled == value) return;

                    if (value)
                    {
                        Start();
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }

        public uint Interval { get; set; }

        #endregion

        #region Events

        private void OnTick(IntPtr lpParameter, bool timerOrWaitFired)
        {
            if (Tick != null)
            {
                Tick(this, EventArgs.Empty);
            }
        }

        public event EventHandler Tick;

        #endregion

        #region Public methods

        public void Start()
        {
            lock (this)
            {
                if (!Enabled)
                {
                    CreateTimerQueueTimer(out _timerHandle, IntPtr.Zero, _callback, IntPtr.Zero, Interval, Interval,
                        ExecuteFlags.WT_EXECUTEDEFAULT);
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (Enabled)
                {
                    DeleteTimerQueueTimer(IntPtr.Zero, _timerHandle, IntPtr.Zero);
                    _timerHandle = IntPtr.Zero;
                }
            }
        }

        #endregion

        #region P/Invoke

        private delegate void WaitOrTimerDelegate(IntPtr lpParameter, bool timerOrWaitFired);

        private enum ExecuteFlags : uint
        {
            /// <summary>
            ///     By default, the callback function is queued to a non-I/O worker thread.
            /// </summary>
            WT_EXECUTEDEFAULT = 0x00000000,

            /// <summary>
            ///     The callback function is invoked by the timer thread itself. This flag should be used only for short tasks or it
            ///     could affect other timer operations.
            ///     The callback function is queued as an APC. It should not perform alertable wait operations.
            /// </summary>
            WT_EXECUTEINTIMERTHREAD = 0x00000020,

            /// <summary>
            ///     The callback function is queued to an I/O worker thread. This flag should be used if the function should be
            ///     executed in a thread that waits in an alertable state.
            ///     The callback function is queued as an APC. Be sure to address reentrancy issues if the function performs an
            ///     alertable wait operation.
            /// </summary>
            WT_EXECUTEINIOTHREAD = 0x00000001,

            /// <summary>
            ///     The callback function is queued to a thread that never terminates. It does not guarantee that the same thread is
            ///     used each time. This flag should be used only for short tasks or it could affect other timer operations.
            ///     Note that currently no worker thread is truly persistent, although no worker thread will terminate if there are any
            ///     pending I/O requests.
            /// </summary>
            WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,

            /// <summary>
            ///     The callback function can perform a long wait. This flag helps the system to decide if it should create a new
            ///     thread.
            /// </summary>
            WT_EXECUTELONGFUNCTION = 0x00000010,

            /// <summary>
            ///     The timer will be set to the signaled state only once. If this flag is set, the Period parameter must be zero.
            /// </summary>
            WT_EXECUTEONLYONCE = 0x00000008,

            /// <summary>
            ///     Callback functions will use the current access token, whether it is a process or impersonation token. If this flag
            ///     is not specified, callback functions execute only with the process token.
            ///     Windows XP/2000:  This flag is not supported until Windows XP with SP2 and Windows Server 2003.
            /// </summary>
            WT_TRANSFER_IMPERSONATION = 0x00000100
        };
        
        [DllImport("kernel32.dll")]
        private static extern bool CreateTimerQueueTimer(out IntPtr phNewTimer,
            IntPtr timerQueue, WaitOrTimerDelegate callback, IntPtr parameter,
            uint dueTime, uint period, ExecuteFlags flags);

        [DllImport("kernel32.dll")]
        private static extern bool DeleteTimerQueueTimer(IntPtr timerQueue, IntPtr timer,
            IntPtr completionEvent);

        #endregion
    }
}
