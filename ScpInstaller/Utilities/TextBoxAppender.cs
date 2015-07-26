using System;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace ScpDriver.Utilities
{
    public class TextBoxAppender : AppenderSkeleton
    {
        private Form _form;
        private TextBox _textBox;
        public string FormName { get; set; }
        public string TextBoxName { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_textBox == null)
            {
                if (string.IsNullOrEmpty(FormName) ||
                    string.IsNullOrEmpty(TextBoxName))
                    return;

                // get desired Form from config
                _form = Application.OpenForms[FormName];
                if (_form == null)
                    return;

                // get desired TextBox control from config
                _textBox = _form.Controls[TextBoxName] as TextBox;
                if (_textBox == null)
                    return;

                _form.FormClosing += (s, e) => _textBox = null;
            }

            // check if called outside of main GUI thread
            if (_textBox.InvokeRequired)
            {
                if (_form != null)
                {
                    // queue method invokation on main thread
                    _form.Invoke(new Action<LoggingEvent>(Append), loggingEvent);
                    return;
                }
            }

            // append message to TextBox control
            _textBox.AppendText(string.Format("{0} [{3}] - {1}{2}", loggingEvent.TimeStamp, loggingEvent.RenderedMessage,
                Environment.NewLine, loggingEvent.Level));
        }
    }
}