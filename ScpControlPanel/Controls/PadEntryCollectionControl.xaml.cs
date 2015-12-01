using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using AutoDependencyPropertyMarker;
using ScpControl.Profiler;
using ScpControl.ScpCore;

namespace ScpControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for PadEntryCollectionControl.xaml
    /// </summary>
    public partial class PadEntryCollectionControl : UserControl
    {
        [AutoDependencyProperty]
        public ObservableCollection<PadEntryControl> PadEntryCollection { get; set; }

        public PadEntryCollectionControl()
        {
            InitializeComponent();

            PadEntryCollection = new ObservableCollection<PadEntryControl>();
            PadEntryCollection.CollectionChanged += PadEntryCollectionOnCollectionChanged;
        }

        private void PadEntryCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            foreach (PadEntryControl padEntryControl in notifyCollectionChangedEventArgs.NewItems)
            {
                padEntryControl.IsTopPad = (padEntryControl.PadId != DsPadId.One && padEntryControl.PadId != DsPadId.None);
            }
        }
    }
}
