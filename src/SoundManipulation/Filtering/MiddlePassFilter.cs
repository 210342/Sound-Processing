using System;
using System.Collections.Generic;
using System.Text;

namespace SoundManipulation.Filtering
{
    public class MiddlePassFilter : Filter
    {
        public MiddlePassFilter(int filterLength, int cutoffFrequency, bool isCausal)
            : base(filterLength, cutoffFrequency, isCausal) { }

        public override double GetKCoefficient(decimal sampleRate, decimal cutoffFrequency) =>
            Convert.ToDouble(4 * sampleRate / (sampleRate - 4 * cutoffFrequency));

        public override double GetValue(int sampleIndex) => 2.0 * Math.Sin(Math.PI * sampleIndex / 2.0);
    }
}
