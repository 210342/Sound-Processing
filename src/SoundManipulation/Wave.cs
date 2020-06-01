using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
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
        public IEnumerable<double> HorizontalAxis { get; }
        public int SamplesCount { get; }
        public decimal SamplePeriod { get; }
        public decimal SampleRate => 1 / SamplePeriod;
        public decimal? Frequency => 1 / Period;
        public bool IsComplex { get; }

        public IWave FourierTransform
        {
            get
            {
                if (_fourierTransform is null)
                {
                    _fourierTransform = new Wave(
                        CalculateFourierTransform().Samples.Select(c => new Complex(Math.Log10(c.Magnitude), c.Phase)), SamplePeriod
                    );
                }
                return _fourierTransform;
            }
            private set => _fourierTransform = value;
        }

        public IWave WindowedCepstrum { get; private set; }

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

        private IEnumerable<decimal?> _fundamentalFrequencies = Enumerable.Empty<decimal?>();
        public IEnumerable<decimal?> FundamentalFrequencies
        {
            get => _fundamentalFrequencies;
            set
            {
                _fundamentalFrequencies = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FundamentalFrequencies)));
            }
        }

        #endregion

        #region Constructors

        public Wave(IEnumerable<Complex> samples, decimal samplePeriod) : this(samples, samplePeriod, true) { }

        public Wave(IEnumerable<Complex> samples, decimal samplePeriod, bool isComplex)
            : this(samples, Enumerable.Range(0, samples.Count()).Select(i => (double)(i * samplePeriod)), samplePeriod, isComplex) { }

        public Wave(IEnumerable<double> samples, decimal samplePeriod) : this(samples.Select(x => new Complex(x, 0)), samplePeriod, false) { }

        public Wave(IEnumerable<Complex> samples, IEnumerable<double> horizontalAxis, decimal samplePeriod, bool isComplex)
        {
            _samples = samples.ToArray();
            SamplesCount = _samples.Count();
            SamplePeriod = samplePeriod;
            IsComplex = isComplex;
            HorizontalAxis = horizontalAxis;
        }

        #endregion

        #region API

        public IWave Calculate(IWave other, Func<Complex, Complex, Complex> operation) =>
            new Wave(other.Samples.Zip(Samples, operation), SamplePeriod);

        public IWave Add(IWave other) => Calculate(other, (lhs, rhs) => lhs + rhs);

        public IWave Subtract(IWave other) => Calculate(other, (lhs, rhs) => lhs - rhs);

        public IWave Multiply(IWave other) => Calculate(other, (lhs, rhs) => lhs * rhs);

        public IWave Concatenate(IWave other) =>
            new Wave(Samples.Concat(other.Samples), SamplePeriod);

        public IEnumerable<decimal?> GetFrequencies(string methodName, int windowSize, double accuracy)
        {
            IList<decimal?> frequencies = new List<decimal?>();
            for (int i = 0; i < _samples.Length; i += windowSize)
            {
                IWave windowed = new Wave(_samples.Skip(i).Take(windowSize), SamplePeriod);
                object result = GetType().GetMethod(methodName).Invoke(windowed, new object[] { accuracy });
                frequencies.Add(result as decimal?);
                if (WindowedCepstrum is null)
                {
                    WindowedCepstrum = windowed.WindowedCepstrum;
                }
            }
            return frequencies;
        }

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
                    return 1 / ((m - 1) * SamplePeriod);
                }
            }
            return null;
        }


        public decimal? CepstralAnalysis(double accuracy)
        {
            IWave fourier = CalculateFourierTransform();
            IWave magnitude = new Wave(fourier.Magnitude, fourier.SamplePeriod);
            IWave cepstral = magnitude.CalculateInverseFourierTransform();
            IWave scaledCepstral = new Wave(
                cepstral.Samples.Select(c => Complex.FromPolarCoordinates(Math.Log(c.Magnitude), c.Phase)),
                Enumerable.Range(1, cepstral.SamplesCount + 1).Select(i => (double)(SampleRate / i)),
                SamplePeriod,
                true
            );
            WindowedCepstrum = scaledCepstral;
            (FourierTransform as Wave).FourierTransform = scaledCepstral;
            return SampleRate / scaledCepstral.GetIndexOfGlobalMaximum(c => c.Magnitude);
        }

        public int? GetIndexOfGlobalMaximum(Func<Complex, double> selector)
        {
            double threshold = 0.85 * selector(_samples[0]);
            int min = 0;
            for (int i = 1; i < _samples.Length; ++i)
            {
                if (selector(_samples[i]) < selector(_samples[min]) || selector(_samples[i]) > threshold)
                {
                    min = i;
                }
                else
                {
                    break;
                }
            }
            int max = min + 1;
            for (int i = min + 2; i < _samples.Length / 2; ++i)
            {
                if (selector(_samples[i]) > selector(_samples[max]))
                {
                    max = i;
                }
            }
            return max;
        }

        public IWave CalculateFourierTransform()
        {
            Complex[] newArray = _samples.ToArray();
            Fourier.Forward(newArray);
            return new Wave(
                newArray, 
                Enumerable.Range(0, newArray.Length).Select(i =>(double)(i * SampleRate / SamplesCount)), 
                SamplePeriod,
                true
            );
        }

        public IWave CalculateInverseFourierTransform()
        {
            Complex[] newArray = _samples.ToArray();
            Fourier.Inverse(newArray);
            return new Wave(
                newArray,
                Enumerable.Range(0, newArray.Length).Select(i => (double)(i * SampleRate / SamplesCount)),
                SamplePeriod,
                true
            );
        }

        public static async Task<Wave> ReadFromStreamAsync(Stream stream)
        {
            using (WaveFileReader reader = new WaveFileReader(stream))
            {
                byte[] bytes = new byte[reader.Length];
                double[] samples = new double[reader.Length / reader.BlockAlign];
                await reader.ReadAsync(bytes, 0, (int)Math.Min(reader.Length, int.MaxValue));
                int bytesIterator = 0;
                for (int i = 0; i < samples.Length; ++i)
                {
                    byte[] sample = new byte[4];
                    bytes.AsSpan(bytesIterator, reader.BlockAlign).CopyTo(sample.AsSpan(4 - reader.BlockAlign));
                    samples[i] = BitConverter.ToInt32(sample, 0);
                    bytesIterator += reader.BlockAlign;
                }
                double samplePeriod = reader.TotalTime.TotalSeconds / samples.Length;
                return new Wave(samples, Convert.ToDecimal(samplePeriod));
            }
        }

        #endregion

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
