using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScpCustomHidProfiler
{
    /// <summary>
    /// Interaction logic for ButtonMappingEntryControl.xaml
    /// </summary>
    public partial class ButtonMappingEntryControl : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register
        (
            "ImageSource",
            typeof(Uri),
            typeof(ButtonMappingEntryControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnImageSourceChanged))
        );

        public Uri ImageSource
        {
            get { return (Uri)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public ButtonMappingEntryControl()
        {
            InitializeComponent();
        }

        private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var myUserControl = sender as ButtonMappingEntryControl;
            if (myUserControl != null)
            {
                myUserControl.ButtonImage.Source = new BitmapImage((Uri)e.NewValue);
            }
        }
    }
}
