using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ScpControl.Value_Converters
{
    public class MultiCheckedToEnabledConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                return values.OfType<bool>().Any(b => b);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] {};
        }

        #endregion
    }
}
