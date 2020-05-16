using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace SoundManipulation
{
    public class Wave : IWave
    {
        private decimal? _period;
        private readonly Complex[] _samples;
        protected static readonly double EPSILON = 1E-6;

        public IEnumerable<Complex> Samples => _samples;
        public IEnumerable<double> Real => Samples.Select(c => c.Real);
        public IEnumerable<double> Imaginary => Samples.Select(c => c.Imaginary);
        public IEnumerable<double> Magnitude => Samples.Select(c => c.Magnitude);
        public IEnumerable<double> Phase => Samples.Select(c => c.Phase);
        public decimal SamplePeriod { get; }
        public decimal SampleRate => 1 / SamplePeriod;
        public decimal Frequency { get; }
        public decimal Period
        {
            get
            {
                if (!_period.HasValue)
                {
                    _period = AMDF();
                }
                return _period.Value;
            }
        }

        public Wave(IEnumerable<Complex> samples, decimal samplePeriod, decimal frequency)
        {
            _samples = samples.ToArray();
            SamplePeriod = samplePeriod;
            Frequency = frequency;
        }

        public Wave(IEnumerable<double> samples, decimal samplePeriod, decimal frequency)
        {
            _samples = samples.Select(x => new Complex(x, 0)).ToArray();
            SamplePeriod = samplePeriod;
            Frequency = frequency;
        }

        public IWave Calculate(IWave other, Func<Complex, Complex, Complex> operation) =>
            new Wave(other.Samples.Zip(Samples, operation), SamplePeriod, Frequency);

        public IWave Add(IWave other) => Calculate(other, (lhs, rhs) => lhs + rhs);

        public IWave Subtract(IWave other) => Calculate(other, (lhs, rhs) => lhs - rhs);

        public IWave Multiply(IWave other) => Calculate(other, (lhs, rhs) => lhs * rhs);

        public IWave Concatenate(IWave other) =>
            new Wave(Samples.Concat(other.Samples), SamplePeriod, Frequency);

        public decimal AMDF()
        {
            double[] delayedArray = new double[_samples.Length];
            delayedArray[0] = 0;
            int? minimumIndex = null;
            for (int m = 1; m < _samples.Length; ++m)
            {
                double sum = 0;
                for (int i = 0; i < _samples.Length; ++i)
                {
                    Complex delayed = _samples[Math.Min(_samples.Length - 1, i + m)];
                    sum += Complex.Abs(_samples[i] - delayed);
                }
                delayedArray[m] = sum;
                if (!minimumIndex.HasValue || delayedArray[minimumIndex.Value] < sum)
                {
                    minimumIndex = m;
                }
            }
            return minimumIndex.Value * SamplePeriod;
        }

        public IWave CalculateFourierTransform()
        {
            Complex[] newArray = ExtendSamplesToLengthOfPowerTwo();
            Fourier.Forward(newArray);
            return new Wave(newArray, SamplePeriod, Frequency);
        }

        public IWave CalculateInverseFourierTransform()
        {
            Complex[] newArray = ExtendSamplesToLengthOfPowerTwo();
            Fourier.Inverse(newArray);
            return new Wave(newArray, SamplePeriod, Frequency);
        }

        protected internal int GetCeilingPowerExponent()
        {
            double exponent = Math.Log(_samples.Length, 2);
            return exponent - (int)exponent < EPSILON ? (int)exponent : (int)(Math.Ceiling(exponent) + 0.01);
        }

        protected internal Complex[] ExtendSamplesToLengthOfPowerTwo()
        {
            int integerExponent = GetCeilingPowerExponent();
            return Samples
                .Concat(Samples.Take((int)(Math.Pow(2, integerExponent) + 0.001) - _samples.Length))
                .ToArray();
        }
    }
}
