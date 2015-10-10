using System.Windows;
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
        }
    }
}
