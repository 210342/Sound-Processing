using System;
using System.Collections.Generic;
using System.Text;

namespace SoundManipulation.Filtering
{
    public class LowPassFilter : Filter
    {
        public LowPassFilter(int filterLength, int cutoffFrequency, bool isCausal) 
            : base(filterLength, cutoffFrequency, isCausal) { }

        public override double GetKCoefficient(decimal sampleRate, decimal cutoffFrequency) =>
            Convert.ToDouble(sampleRate / cutoffFrequency);

        public override double GetValue(int sampleIndex) => 1.0;
    }
}
