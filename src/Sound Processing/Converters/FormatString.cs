using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SoundProcessing.Converters
{
    public class FormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string formatString = parameter?.ToString();
            return value is null || string.IsNullOrEmpty(formatString)
                ? string.Empty
                : string.Format(formatString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
