using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using MathNet.Numerics;

namespace SoundManipulation
{
    public class Wave : IWave
    {
        private IWave _fourierTransform;
        private readonly Complex[] _samples;
        protected static readonly double EPSILON = 1E-6;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        public IEnumerable<Complex> Samples => _samples;
        public IEnumerable<double> Real => Samples.Select(c => c.Real);
        public IEnumerable<double> Imaginary => Samples.Select(c => c.Imaginary);
        public IEnumerable<double> Magnitude => Samples.Select(c => c.Magnitude);
        public IEnumerable<double> Phase => Samples.Select(c => c.Phase);
        public int SamplesCount { get; }
        public decimal SamplePeriod { get; }
        public decimal SampleRate => 1 / SamplePeriod;
        public decimal? Frequency => 1 / Period;

        public IWave FourierTransform
        {
            get
            {
                if (_fourierTransform is null)
                {
                    _fourierTransform = CalculateFourierTransform();
                }
                return _fourierTransform;
            }
        }

        #endregion

        #region Observable properties

        private decimal? _period;
        public decimal? Period 
        { 
            get { return _period; }
            set
            {
                _period = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Period)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Frequency)));
            }
        }

        #endregion

        public Wave(IEnumerable<Complex> samples, decimal samplePeriod)
        {
            _samples = samples.ToArray();
            SamplesCount = _samples.Count();
            SamplePeriod = samplePeriod;
        }

        public Wave(IEnumerable<double> samples, decimal samplePeriod)
        {
            _samples = samples.Select(x => new Complex(x, 0)).ToArray();
            SamplesCount = _samples.Count();
            SamplePeriod = samplePeriod;
        }

        public IWave Calculate(IWave other, Func<Complex, Complex, Complex> operation) =>
            new Wave(other.Samples.Zip(Samples, operation), SamplePeriod);

        public IWave Add(IWave other) => Calculate(other, (lhs, rhs) => lhs + rhs);

        public IWave Subtract(IWave other) => Calculate(other, (lhs, rhs) => lhs - rhs);

        public IWave Multiply(IWave other) => Calculate(other, (lhs, rhs) => lhs * rhs);

        public IWave Concatenate(IWave other) =>
            new Wave(Samples.Concat(other.Samples), SamplePeriod);

        public decimal? AMDF(double accuracy)
        {
            double[] delayedArray = new double[_samples.Length];
            delayedArray[0] = 0;
            delayedArray[1] = 1;
            for (int m = 2; m < _samples.Length; ++m)
            {
                double sum = 0;
                for (int i = 0; i < _samples.Length; ++i)
                {
                    Complex delayed = _samples[Math.Min(_samples.Length - 1, i + m)];
                    sum += Complex.Abs(_samples[i] - delayed);
                }
                delayedArray[m] = sum;
                if (delayedArray[m - 2] - delayedArray[m - 1] > accuracy * delayedArray[m - 2]
                    && delayedArray[m] - delayedArray[m - 1] > accuracy * delayedArray[m-1])
                {
                    return (m - 1) * SamplePeriod;
                }
            }
            return null;
        }

        public decimal? CepstralAnalysis()
        {
            IWave cepstral = FourierTransform.FourierTransform;
            decimal period = Convert.ToDecimal(cepstral.Magnitude.Aggregate((x1, x2) => x1 > x2 ? x1 : x2));
            return period;
        }

        public IWave CalculateFourierTransform()
        {
            Complex[] newArray = ExtendSamplesToLengthOfPowerOfTwo();
            Fourier.Forward(newArray);
            return new Wave(newArray.Take(newArray.Length / 2), SamplePeriod);
        }

        public IWave CalculateInverseFourierTransform()
        {
            Complex[] newArray = ExtendSamplesToLengthOfPowerOfTwo();
            Fourier.Inverse(newArray);
            return new Wave(newArray, SamplePeriod);
        }

        public static async Task<Wave> ReadFromStreamAsync(Stream stream)
        {
            using (WaveFileReader reader = new WaveFileReader(stream))
            {
                byte[] bytes = new byte[reader.Length];
                double[] samples = new double[reader.Length / 2];
                await reader.ReadAsync(bytes, 0, (int)Math.Min(reader.Length, int.MaxValue));
                int bytesIterator = 0;
                for (int i = 0; i < bytes.Length / 2; ++i)
                {
                    samples[i] = BitConverter.ToInt16(bytes, bytesIterator);
                    bytesIterator += 2;
                }
                double samplePeriod = reader.TotalTime.TotalSeconds / samples.Length;
                return new Wave(samples, Convert.ToDecimal(samplePeriod));
            }
        }

        public void SaveToStream(Stream stream)
        {
            using (WaveFileWriter writer = new WaveFileWriter(stream, new WaveFormat()))
            {

            }
        }

        protected internal int GetCeilingPowerExponent()
        {
            double exponent = Math.Log(_samples.Length, 2);
            return exponent - (int)exponent < EPSILON ? (int)exponent : (int)(Math.Ceiling(exponent) + 0.01);
        }

        protected internal Complex[] ExtendSamplesToLengthOfPowerOfTwo()
        {
            int integerExponent = GetCeilingPowerExponent();
            return Samples
                .Concat(Samples.Take((int)(Math.Pow(2, integerExponent) + 0.001) - _samples.Length))
                .ToArray();
        }
    }
}
