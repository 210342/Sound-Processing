using System;
using System.Collections.Generic;
using System.Text;

namespace SoundManipulation.Filtering
{
    public interface IFilter
    {
        int FilterLength { get; }
        bool IsCausal { get; }
        int CutoffFrequency { get; }

        double GetValue(int sampleIndex);
        double GetKCoefficient(decimal sampleRate, decimal cutoffFrequency);
    }
}
