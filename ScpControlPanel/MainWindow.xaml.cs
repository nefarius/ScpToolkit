using System.Windows;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControlPanel.Controls;

namespace ScpControlPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainPadEntryCollectionControl.PadEntryCollection.Add(new PadEntryControl());

            MainPadEntryCollectionControl.PadEntryCollection[0].ConnectionType = DsConnection.BTH;
            MainPadEntryCollectionControl.PadEntryCollection[0].PadId = DsPadId.One;

            MainPadEntryCollectionControl.PadEntryCollection.Add(new PadEntryControl() { PadId = DsPadId.Two });
            MainPadEntryCollectionControl.PadEntryCollection.Add(new PadEntryControl() { PadId = DsPadId.Three });
            MainPadEntryCollectionControl.PadEntryCollection.Add(new PadEntryControl() { PadId = DsPadId.Four });
        }
    }
}
