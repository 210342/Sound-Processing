using System;
using System.Collections.Generic;
using System.Text;

namespace SoundManipulation.Filtering
{
    public class HighPassFilter : Filter
    {
        public HighPassFilter(int filterLength, int cutoffFrequency, bool isCausal)
            : base(filterLength, cutoffFrequency, isCausal) { }

        public override double GetKCoefficient(decimal sampleRate, decimal cutoffFrequency)
            => Convert.ToDouble(sampleRate / (sampleRate / 2 - cutoffFrequency));

        public override double GetValue(int sampleIndex)
        {
            throw new NotImplementedException();
        }
    }
}
