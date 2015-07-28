using System;
using System.Linq;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace ScpServer.Utilities
{
    public class ListViewAppender : AppenderSkeleton
    {
        private Form _form;
        private ListView _listView;
        public string FormName { get; set; }
        public string ListViewName { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_listView == null)
            {
                if (string.IsNullOrEmpty(FormName) ||
                    string.IsNullOrEmpty(ListViewName))
                    return;

                // get desired Form from config
                _form = Application.OpenForms[FormName];
                if (_form == null)
                    return;

                // get desired ListView control from config
                _listView = _form.Controls.Find(ListViewName, true).First() as ListView;
                if (_listView == null)
                    return;

                _form.FormClosing += (s, e) => _listView = null;
            }

            // check if called outside of main GUI thread
            if (_listView.InvokeRequired)
            {
                if (_form != null && !_form.IsDisposed)
                {
                    try
                    {
                        // queue method invokation on main thread
                        _form.BeginInvoke(new Action<LoggingEvent>(Append), loggingEvent);
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                }
            }

            // append message to ListView control
            _listView.Items.Add(
                new ListViewItem(new[]
                {loggingEvent.TimeStamp.ToString(), loggingEvent.Level.ToString(), loggingEvent.RenderedMessage}))
                .EnsureVisible();
        }
    }
}