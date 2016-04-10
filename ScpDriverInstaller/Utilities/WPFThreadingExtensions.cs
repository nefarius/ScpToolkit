using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace ScpDriverInstaller.Utilities
{
    public static class WPFThreadingExtensions
    {
        /// <summary>
        /// Simple helper extension method to marshall to correct
        /// thread if its required
        /// </summary>
        /// <param name="control">The source control</param>
        /// <param name="methodcall">The method to call</param>
        /// <param name="priorityForCall">The thread priority</param>
        public static void InvokeIfRequired(this DispatcherObject control, Action methodcall, DispatcherPriority priorityForCall)
        {
            //see if we need to Invoke call to Dispatcher thread  
            if (control.Dispatcher.Thread != Thread.CurrentThread)
                control.Dispatcher.Invoke(priorityForCall, methodcall);
            else
                methodcall();
        }

        /// <summary>
        /// Gets the Visibility of a UIElement in a thread safe way
        /// </summary>
        /// <param name="uie">The UIElement</param>
        /// <returns>The Visibility of the UIElement</returns>
        public static Visibility GetVisibilityThreadSafe(this UIElement uie)
        {
            Visibility vis = Visibility.Hidden;
            InvokeIfRequired(uie, () => { vis = uie.Visibility; }, DispatcherPriority.Background);
            return vis;
        }

        /// <summary>
        /// Sets the Visibility of a UIElement in a thread safe way
        /// </summary>
        /// <param name="uie">The UIElement</param>
        /// <param name="vis">The required Visibility</param>
        public static void SetVisibilityThreadSafe(this UIElement uie, Visibility vis)
        {
            InvokeIfRequired(uie, () => { uie.Visibility = vis; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Sets the IsEnabled value in a thread safe way
        /// </summary>
        /// <param name="uie">Thr UIElement</param>
        /// <param name="enabled">The value for the IsEnabled property</param>
        public static void SetIsEnabledThreadSafe(this UIElement uie, bool enabled)
        {
            InvokeIfRequired(uie, () => { uie.IsEnabled = enabled; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Sets the text of a TextBlock in a thread safe way
        /// </summary>
        /// <param name="tb">The TextBlock</param>
        /// <param name="s">The string for the Text property</param>
        public static void SetTextThreadSafe(this TextBlock tb, string s)
        {
            InvokeIfRequired(tb, () => { tb.Text = s; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Gets the Text of a TextBox in a thread safe way
        /// </summary>
        /// <param name="tb">The TextBox</param>
        /// <returns>A string containing the Text of the TextBox</returns>
        public static string SetTextThreadSafe(this TextBox tb)
        {
            string s = "";
            InvokeIfRequired(tb, () => { s = tb.Text; }, DispatcherPriority.Background);
            return s;
        }

        /// <summary>
        /// Sets the text of a TextBox in a thread safe way
        /// </summary>
        /// <param name="tb">The TextBox</param>
        /// <param name="s">The string for the Text property</param>
        public static void SetTextThreadSafe(this TextBox tb, string s)
        {
            InvokeIfRequired(tb, () => { tb.Text = s; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Gets the Content of a ContentControl in a thread safe way
        /// </summary>
        /// <param name="cc">The ContentControl</param>
        /// <returns>A string containing the Content of the ContentControl</returns>
        public static string GetContentThreadSafe(this ContentControl cc)
        {
            string s = "";
            InvokeIfRequired(cc, () => { s = cc.Content.ToString(); }, DispatcherPriority.Background);
            return s;
        }

        /// <summary>
        /// Sets the Content of a ContentControl in a thread safe way
        /// </summary>
        /// <param name="cc">The ContentControl</param>
        /// <param name="s">The string to use as the Content of the ContentControl</param>
        public static void SetContentThreadSafe(this ContentControl cc, string s)
        {
            InvokeIfRequired(cc, () => { cc.Content = s; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Gets the value of the IsChecked property of a CheckBox in a thread safe way
        /// </summary>
        /// <param name="cb">The CheckBox</param>
        /// <returns>A bool containing the value if the IsChecked property</returns>
        public static bool GetIsCheckedThreadSafe(this CheckBox cb)
        {
            bool? val = null;
            InvokeIfRequired(cb, () => { val = cb.IsChecked; }, DispatcherPriority.Background);
            return (bool)val;
        }

        /// <summary>
        /// Gets the value of the IsChecked property of a RadioButton in a thread safe way
        /// </summary>
        /// <param name="rb">The RadioButton</param>
        /// <returns>A bool containing the value if the IsChecked property</returns>
        public static bool GetIsCheckedThreadSafe(this RadioButton rb)
        {
            bool? val = null;
            InvokeIfRequired(rb, () => { val = rb.IsChecked; }, DispatcherPriority.Background);
            return (bool)val;
        }

        /// <summary>
        /// Sets the value of the IsChecked property of a RadioButton in a thread safe way
        /// </summary>
        /// <param name="rb">The RadioButton</param>
        /// <param name="val">The value to set the IsChecked property to</param>
        public static void SetIsCheckedThreadSafe(this RadioButton rb, bool? val)
        {
            InvokeIfRequired(rb, () => { rb.IsChecked = val; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Sets the value of the IsChecked property of a CheckBox in a thread safe way
        /// </summary>
        /// <param name="cb">The CheckBox</param>
        /// <param name="val">The value to set the IsChecked property to</param>
        public static void SetIsCheckedThreadSafe(this CheckBox cb, bool? val)
        {
            InvokeIfRequired(cb, () => { cb.IsChecked = val; }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Gets the Value property of a ProgressBar in a thread safe way
        /// </summary>
        /// <param name="pb">The ProgressBar</param>
        /// <returns>a double containing the Value property of the ProgressBar</returns>
        public static double GetValueThreadSafe(this ProgressBar pb)
        {
            double val = 0;
            InvokeIfRequired(pb, () => { val = pb.Value; }, DispatcherPriority.Background);
            return val;
        }

        /// <summary>
        /// Sets the Value property of a ProgressBar in a thread safe way
        /// </summary>
        /// <param name="pb">the ProgressBar</param>
        /// <param name="val">The value to Set the Value property to</param>
        public static void SetValueThreadSafe(this ProgressBar pb, double val)
        {
            InvokeIfRequired(pb, () => { pb.Value = val; }, DispatcherPriority.Background);
        }

        public static void SetContentThreadSafe(this BusyIndicator bi, string val)
        {
            InvokeIfRequired(bi, () => { bi.BusyContent = val; }, DispatcherPriority.Background);
        }
    }
}
