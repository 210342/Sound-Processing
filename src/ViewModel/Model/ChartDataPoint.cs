using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Model
{
    public class ChartDataPoint
    {
        public double X { get; }
        public double Y { get; }

        public string Label => string.Format("{0:0.####}", X);

        public ChartDataPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
