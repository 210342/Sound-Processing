using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Model
{
    public class Window
    {
        public (int From, int To) SampleRange { get; }
        public decimal TimeFrom { get; }
        public decimal TimeTo { get; }
        public decimal? Frequency { get; }

        public Window((int, int) sampleRange, decimal timeFrom, decimal timeTo, decimal? frequency)
        {
            SampleRange = sampleRange;
            TimeFrom = timeFrom;
            TimeTo = timeTo;
            Frequency = frequency;
        }
    }
}
