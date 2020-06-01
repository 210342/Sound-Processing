using SoundManipulation.Filtering;
using System;
using System.Linq;

namespace SoundManipulation
{
    public static class WaveFactory
    {
        public static IWave WaveWithFrequency(double frequency, decimal samplePeriod, int sampleCount)
            => WaveWithFrequency(frequency, (double)samplePeriod, sampleCount);

        public static IWave WaveWithFrequency(double frequency, double samplePeriod, int sampleCount)
        {
            double twoPi = 2 * Math.PI;
            var samples = Enumerable
                .Range(0, sampleCount)
                .Select(i => Math.Sin(i * samplePeriod * twoPi * frequency));
            return new Wave(samples, Convert.ToDecimal(samplePeriod))
            {
                Period = Convert.ToDecimal(1 / frequency)
            };
        }

        public static double[] GetWindowValues(WindowDelegate window, int windowSize)
        {
            return Enumerable.Range(0, windowSize).Select(i => window(i, windowSize)).ToArray();
        }

        public static IWave GetFilter(IFilter filterType, WindowDelegate window, int filterLength, decimal cutoffFrequency, decimal sampleRate)
        {
            double k = filterType.GetKCoefficient(sampleRate, cutoffFrequency);
            double[] values = new double[filterLength];

            int halfFilterLength = (filterLength - 1) / 2;
            double twoPi = 2 * Math.PI;

            for (int i = 0; i < filterLength; i++)
            {
                if (i == halfFilterLength)
                {
                    values[i] = 2.0 / k;
                }
                else
                {
                    values[i] = Math.Sin(twoPi * (i - halfFilterLength) / k) / (Math.PI * (i - halfFilterLength));
                }
                values[i] *= window(i, filterLength);
                values[i] *= filterType.GetValue(i - halfFilterLength);
            }
            return new Wave(values, 1 / sampleRate);
        }
    }
}
