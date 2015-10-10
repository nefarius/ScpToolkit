using System.Windows;
using System.Windows.Controls;
using AutoDependencyPropertyMarker;
using ScpControl.ScpCore;

namespace ScpControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for PadEntryControl.xaml
    /// </summary>
    public partial class PadEntryControl : UserControl
    {
        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsPadId PadId { get; set; }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsModel PadType { get; set; }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public string MacAddress { get; set; }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsConnection ConnectionType { get; set; }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public int PacketCounter { get; set; }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsBattery BatteryStatus { get; set; }

        public PadEntryControl()
        {
            InitializeComponent();

            PadId = DsPadId.None;
            PadType = DsModel.DS3;
            MacAddress = "00:00:00:00:00:00";
            ConnectionType = DsConnection.None;
            PacketCounter = 0;
            BatteryStatus = DsBattery.None;
        }
    }
}
