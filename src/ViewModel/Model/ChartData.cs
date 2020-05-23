using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ViewModels.Model
{
    public class ChartData : INotifyPropertyChanged
    {
        public IEnumerable<ChartDataPoint> DataPoints { get; }

        private string _seriesTitle = string.Empty;
        public string SeriesTitle
        {
            get { return _seriesTitle; }
            set
            {
                _seriesTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeriesTitle)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ChartData(IEnumerable<double> horizontal, IEnumerable<double> values, string title)
        {
            SeriesTitle = title;
            DataPoints = horizontal.Zip(values, (x, y) => new ChartDataPoint(x, y));
        }
    }
}
