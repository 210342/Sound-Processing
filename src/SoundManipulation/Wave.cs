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
using SoundManipulation.Filtering;
using NAudio.Utils;

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
                    _fourierTransform = CalculateFourierTransform();
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
            get => _period;
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
                Period = Period ?? 1 / value.FirstOrDefault();
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

        #region Operations

        public IWave Calculate(IWave other, Func<Complex, Complex, Complex> operation) =>
            new Wave(other.Samples.Zip(Samples, operation), SamplePeriod);

        public IWave Add(IWave other) => Calculate(other, (lhs, rhs) => lhs + rhs);

        public IWave Subtract(IWave other) => Calculate(other, (lhs, rhs) => lhs - rhs);

        public IWave Multiply(IWave other) => Calculate(other, (lhs, rhs) => lhs * rhs);

        public IWave Concatenate(IWave other) =>
            new Wave(Samples.Concat(other.Samples), SamplePeriod);

        public IWave Convolve(IWave other)
        {
            if (!(other is Wave rhs))
            {
                return null;
            }

            int lastIndex = other.SamplesCount - 1;
            int sampleCount = SamplesCount + lastIndex;
            Complex[] values = new Complex[sampleCount];
            for (int i = 0; i < sampleCount; ++i)
            {
                int leftOperandIndex = Math.Max(0, i - lastIndex);
                int rightOperandIndex = Math.Min(i, lastIndex);
                while (leftOperandIndex < SamplesCount && rightOperandIndex >= 0)
                {
                    if (rightOperandIndex < other.SamplesCount)
                    {
                        values[i] += _samples[leftOperandIndex] * rhs._samples[rightOperandIndex];
                    }
                    ++leftOperandIndex;
                    --rightOperandIndex;
                }
            }
            return new Wave(values, SamplePeriod);
        }

        #endregion

        #region Frequency analysis

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
            int length = _samples.Length;
            if (length < 2)
            {
                return null;
            }

            double[] delayedArray = new double[length];
            delayedArray[0] = 0;
            delayedArray[1] = 0;
            for (int m = 2; m < length; ++m)
            {
                double sum = 0;
                for (int i = 0; i < _samples.Length; ++i)
                {
                    Complex delayed = _samples[(i + m) % length];
                    sum += (_samples[i] - delayed).Magnitude;
                }
                delayedArray[m] = sum;
                if (delayedArray[m - 2] - delayedArray[m - 1] > accuracy * delayedArray[m - 1]
                    && delayedArray[m] - delayedArray[m - 1] > accuracy * delayedArray[m - 1])
                {
                    return 1 / ((m - 1) * SamplePeriod);
                }
            }
            return null;
        }

        public IWave GenerateAmdfWaveForWindow((int Start, int End) range)
        {
            int length = range.End - range.Start;
            if (length < 2)
            {
                return null;
            }

            double[] delayedArray = new double[length];
            delayedArray[0] = 0;
            delayedArray[1] = 0;
            for (int m = 2; m < length; ++m)
            {
                double sum = 0;
                for (int i = range.Start; i < range.End; ++i)
                {
                    Complex delayed = _samples[(i + m) % length];
                    sum += (_samples[i] - delayed).Magnitude;
                }
                delayedArray[m] = sum;
            }
            return new Wave(delayedArray, 1);
        }

        public decimal? CepstralAnalysis(double accuracy)
        {
            IWave fourier = CalculateFourierTransform();
            IWave magnitude = new Wave(fourier.Magnitude, fourier.SamplePeriod);
            IWave cepstral = magnitude.CalculateInverseFourierTransform();
            Wave scaledCepstral = new Wave(
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

        public IWave GenerateWaveOfFundamentalFrequencies(int windowSize)
        {
            var waves = FundamentalFrequencies
                .Select((f, i) => WaveFactory.WaveWithFrequency(
                    Real.Skip(i * windowSize).Take(windowSize),
                    (double?)f,
                    SamplePeriod,
                    windowSize
                ));
            return new Wave(waves.SelectMany(wave => wave.Samples.Select(c => c.Real)), SamplePeriod);
        }

        #endregion

        #region Fourier

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

        #endregion

        #region Filters

        public IWave ApplyFilterByWindowedFourier(IFilter filter, WaveWindow window, int hopSize)
        {
            Wave filterWave = WaveFactory.GetFilterWave(filter, SampleRate, window.Function) as Wave;
            int fourierLength = filter.Length + window.Length - 1;
            filterWave = filterWave.ExtendWaveToLengthByAppendingZeros(fourierLength, filter.IsCausal);
            IEnumerable<Wave> windows = CutIntoWindows(window, hopSize)
                .Select(w => w.ExtendWaveToLengthByAppendingZeros(fourierLength, filter.IsCausal));
            var filteredFouriers = windows.Select(w => w.FourierTransform).Select(f => f.Multiply(filterWave.FourierTransform));
            var filteredWindows = filteredFouriers.Select(f => f.CalculateInverseFourierTransform()).OfType<Wave>();
            return MergeWindowsWithShift(filteredWindows, hopSize);
        }

        #endregion

        #region IO

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

        public async Task WriteToStreamAsync(Stream stream)
        {
            await Task.Run(() => 
            {
                double maxSample = Real.Max();
                using (WaveFileWriter writer = new WaveFileWriter(
                    new IgnoreDisposeStream(stream), 
                    new WaveFormatExtensible((int)decimal.Round(SampleRate), 16, 1)
                ))
                {
                    writer.WriteSamples(Real.Select(d => Convert.ToSingle(d / maxSample)).ToArray(), 0, SamplesCount);
                }
            });
        }

        #endregion

        #endregion

        #region Protected methods

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

        protected internal IEnumerable<Wave> CutIntoWindows(WaveWindow window, int hopSize)
        {
            IList<Wave> windows = new List<Wave>();
            for (int i = 0; i < SamplesCount - window.Length; i += hopSize)
            {
                windows.Add(new Wave(_samples.AsSpan(i, window.Length).ToArray(), SamplePeriod));
            }
            return windows;
        }

        protected internal Wave ExtendWaveToLengthByAppendingZeros(int newSize, bool isCausal)
        {
            Complex[] newSamples = new Complex[newSize];
            int hop = isCausal ? 0 : (int)((newSize - 1) / 2 + 0.5);
            for (int i = 0; i < SamplesCount; ++i)
            {
                newSamples[(hop + i) % newSize] = _samples[i];
            }
            return new Wave(newSamples, SamplePeriod);
        }

        protected internal static Wave MergeWindowsWithShift(IEnumerable<Wave> windows, int shift)
        {
            int outputLength = windows.Count() * shift + windows.FirstOrDefault().SamplesCount - shift;
            Complex[] outputSamples = new Complex[outputLength];
            int hop = 0;
            foreach (Wave window in windows)
            {
                for (int i = 0; i < window.SamplesCount; ++i)
                {
                    outputSamples[hop + i] += window._samples[i];
                }
                hop += shift;
            }
            return new Wave(outputSamples, windows.FirstOrDefault().SamplePeriod);
        }

        #endregion
    }
}
