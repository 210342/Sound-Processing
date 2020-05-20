using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ViewModels.Model
{
    public class ChartData
    {
        public IEnumerable<double> HorizontalAxis { get; set; }
        public IEnumerable<double> VerticalAxis { get; set; }

        public ChartData(IEnumerable<double> horizontal, IEnumerable<double> vertical)
        {
            HorizontalAxis = horizontal;
            VerticalAxis = vertical;
        }
    }
}
