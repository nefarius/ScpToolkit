using System.Diagnostics;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace ScpDriverInstaller.Utilities
{
    public static class ExtendedMessageBox
    {
        public static void Show(Window owner, string title, string instruction, string content, string verbose,
            string footer, TaskDialogIcon icon)
        {
            using (var dialog = new TaskDialog())
            {
                dialog.Width = 240;
                dialog.WindowTitle = title;
                dialog.MainInstruction = instruction;
                dialog.Content = content;
                dialog.ExpandedInformation = verbose;
                dialog.Footer = footer;
                dialog.FooterIcon = icon;
                dialog.EnableHyperlinks = true;
                var okButton = new TaskDialogButton(ButtonType.Ok);
                dialog.Buttons.Add(okButton);
                dialog.HyperlinkClicked += (sender, args) => { Process.Start(args.Href); };
                dialog.ShowDialog(owner);
            }
        }
    }
}
