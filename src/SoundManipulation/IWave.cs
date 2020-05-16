using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SoundManipulation
{
    public interface IWave
    {
        IEnumerable<Complex> Samples { get; }
        IEnumerable<double> Real { get; }
        IEnumerable<double> Imaginary { get; }
        IEnumerable<double> Magnitude { get; }
        IEnumerable<double> Phase { get; }
        decimal SamplePeriod { get; }
        decimal SampleRate { get; }
        decimal Frequency { get; }
        decimal Period { get; }

        IWave Calculate(IWave other, Func<Complex, Complex, Complex> operation);
        IWave Add(IWave other);
        IWave Subtract(IWave other);
        IWave Multiply(IWave other);
        IWave Concatenate(IWave other);
        IWave CalculateFourierTransform();
        IWave CalculateInverseFourierTransform();
        decimal AMDF();
    }
}
