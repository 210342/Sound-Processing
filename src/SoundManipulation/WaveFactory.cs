using SoundManipulation.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundManipulation
{
    public static class WaveFactory
    {
        public static IWave WaveWithFrequency(double? frequency, decimal samplePeriod, int sampleCount)
            => WaveWithFrequency(frequency, (double)samplePeriod, sampleCount);

        public static IWave WaveWithFrequency(double? frequency, double samplePeriod, int sampleCount)
        {
            double twoPi = 2 * Math.PI;
            var samples = frequency.HasValue
                ? Enumerable
                    .Range(0, sampleCount)
                    .Select(i => Math.Sin(i * samplePeriod * twoPi * frequency.Value))
                : Enumerable.Repeat(0.0, sampleCount);
            return new Wave(samples, Convert.ToDecimal(samplePeriod))
            {
                Period = Convert.ToDecimal(1 / frequency)
            };
        }

        public static IWave WaveWithFrequency(IEnumerable<double> samples, double? frequency, decimal samplePeriod, int sampleCount)
            => WaveWithFrequency(samples, frequency, (double)samplePeriod, sampleCount);

        public static IWave WaveWithFrequency(IEnumerable<double> samples, double? frequency, double samplePeriod, int sampleCount)
        {
            double twoPi = 2 * Math.PI;
            var newSamples = frequency.HasValue
                ? samples
                    .Select((s, i) => s * Math.Sin(i * samplePeriod * twoPi * frequency.Value))
                : Enumerable.Repeat(0.0, sampleCount);
            return new Wave(newSamples, Convert.ToDecimal(samplePeriod))
            {
                Period = Convert.ToDecimal(1 / frequency)
            };
        }

        public static double[] GetWindowValues(WindowDelegate window, int windowSize)
        {
            return Enumerable.Range(0, windowSize).Select(i => window(i, windowSize)).ToArray();
        }

        public static IWave GetFilterWave(IFilter filterType, decimal sampleRate)
        {
            double k = filterType.GetKCoefficient(sampleRate, filterType.CutoffFrequency);
            double[] values = new double[filterType.Length];

            int halfFilterLength = (filterType.Length - 1) / 2;
            double twoPi = 2 * Math.PI;

            for (int i = 0; i < filterType.Length; i++)
            {
                if (i == halfFilterLength)
                {
                    values[i] = 2.0 / k;
                }
                else
                {
                    values[i] = Math.Sin(twoPi * (i - halfFilterLength) / k) / (Math.PI * (i - halfFilterLength));
                }
                values[i] *= filterType.GetValue(i - halfFilterLength);
            }
            return new Wave(values, 1 / sampleRate);
        }
    }
}
