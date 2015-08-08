using System.ComponentModel;
using System.Text;
using System.Threading;

namespace ScpControl
{
    public partial class UsbDs3AfterglowAp2 : UsbDs3
    {
        public UsbDs3AfterglowAp2()
        {
            InitializeComponent();
        }

        public UsbDs3AfterglowAp2(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        protected override void Parse(byte[] Report)
        {
            var sb = new StringBuilder();

            foreach (var b in Report)
            {
                sb.AppendFormat("{0:X2} ", b);
            }

            Log.DebugFormat("Packet: {0}", sb);
            Thread.Sleep(1000);

            base.Parse(Report);
        }
    }
}