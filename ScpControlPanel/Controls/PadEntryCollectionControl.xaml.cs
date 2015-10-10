using System.Collections.Generic;
using System.Windows.Controls;
using AutoDependencyPropertyMarker;

namespace ScpControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for PadEntryCollectionControl.xaml
    /// </summary>
    public partial class PadEntryCollectionControl : UserControl
    {
        [AutoDependencyProperty]
        public List<PadEntryControl> PadEntryCollection { get; set; }

        public PadEntryCollectionControl()
        {
            InitializeComponent();

            PadEntryCollection = new List<PadEntryControl>();
        }
    }
}
