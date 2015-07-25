using System;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace ScpDriver.Utilities
{
    public class TextBoxAppender : AppenderSkeleton
    {
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

                var form = Application.OpenForms[FormName];
                if (form == null)
                    return;

                _textBox = form.Controls[TextBoxName] as TextBox;
                if (_textBox == null)
                    return;

                form.FormClosing += (s, e) => _textBox = null;
            }

            _textBox.AppendText(string.Format("{0} - {1}{2}", loggingEvent.TimeStamp, loggingEvent.RenderedMessage,
                Environment.NewLine));
        }
    }
}