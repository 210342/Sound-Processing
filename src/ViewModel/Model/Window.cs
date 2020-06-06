using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Model
{
    public class Window
    {
        public decimal TimeFrom { get; }
        public decimal TimeTo { get; }
        public decimal? Frequency { get; }

        public Window(decimal timeFrom, decimal timeTo, decimal? frequency)
        {
            TimeFrom = timeFrom;
            TimeTo = timeTo;
            Frequency = frequency;
        }
    }
}
