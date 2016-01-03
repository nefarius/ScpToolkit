using System.Windows.Controls;
using System.Windows.Media;
using AutoDependencyPropertyMarker;
using ScpControl.Shared.Core;

namespace ScpProfiler
{
    /// <summary>
    ///     Interaction logic for ButtonMappingEntryControl.xaml
    /// </summary>
    public partial class ButtonMappingEntryControl : UserControl
    {
        #region Ctor

        public ButtonMappingEntryControl()
        {
            ButtonProfile = new DsButtonProfile();

            InitializeComponent();
        }

        #endregion

        #region Dependency properties

        [AutoDependencyProperty]
        public ImageSource IconSource { get; set; }

        [AutoDependencyProperty]
        public string IconToolTip { get; set; }

        [AutoDependencyProperty]
        public DsButtonProfile ButtonProfile { get; set; }

        #endregion
    }
}