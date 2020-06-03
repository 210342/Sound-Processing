using SoundManipulation.Filtering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoundManipulation
{
    public interface IWave : INotifyPropertyChanged
    {
        IEnumerable<Complex> Samples { get; }
        IEnumerable<double> Real { get; }
        IEnumerable<double> Imaginary { get; }
        IEnumerable<double> Magnitude { get; }
        IEnumerable<double> Phase { get; }
        IEnumerable<double> HorizontalAxis { get; }
        int SamplesCount { get; }
        decimal SamplePeriod { get; }
        decimal SampleRate { get; }
        decimal? Frequency { get; }
        IEnumerable<decimal?> FundamentalFrequencies { get; set; }
        decimal? Period { get; set; }
        bool IsComplex { get; }
        IWave FourierTransform { get; }
        IWave WindowedCepstrum { get; }

        IWave Calculate(IWave other, Func<Complex, Complex, Complex> operation);
        IWave Add(IWave other);
        IWave Subtract(IWave other);
        IWave Multiply(IWave other);
        IWave Concatenate(IWave other);
        IWave Convolve(IWave other);
        IWave CalculateFourierTransform();
        IWave CalculateInverseFourierTransform();
        IEnumerable<decimal?> GetFrequencies(string methodName, int windowSize, double accuracy);
        decimal? AMDF(double accuracy);
        decimal? CepstralAnalysis(double accuracy);
        int? GetIndexOfGlobalMaximum(Func<Complex, double> selector);
        IWave ApplyFilterByWindowedFourier(IFilter filter, FourierWindow window, int hopSize);
    }
}
