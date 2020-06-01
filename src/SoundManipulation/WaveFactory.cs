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

        // public static IWave GetFilter(double filterLength, double cutoffFrequency, )
    }
}
