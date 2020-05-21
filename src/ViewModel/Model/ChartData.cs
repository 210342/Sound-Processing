using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ViewModels.Model
{
    public class ChartData
    {
        public IEnumerable<ChartDataPoint> DataPoints { get; }
        public IEnumerable<double> X { get; }
        public IEnumerable<double> Y { get; }

        public ChartData(IEnumerable<double> horizontal, IEnumerable<double> values)
        {
            X = horizontal;
            Y = values;
            DataPoints = horizontal.Zip(values, (x, y) => new ChartDataPoint(x, y));
        }
    }
}
