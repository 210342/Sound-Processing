using System;
using System.Collections.Generic;
using System.Text;

namespace SoundManipulation.Filtering
{
    public abstract class Filter : IFilter
    {
        public int FilterLength { get; }
        public bool IsCausal { get; }
        public int CutoffFrequency { get; }

        public Filter(int filterLength, int cutoffFrequency, bool isCausal)
        {
            FilterLength = filterLength;
            CutoffFrequency = cutoffFrequency;
            IsCausal = isCausal;
        }

        public abstract double GetKCoefficient(decimal sampleRate, decimal cutoffFrequency);

        public abstract double GetValue(int sampleIndex);
    }
}
