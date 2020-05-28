﻿using SoundManipulation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.DependencyInjection;
using ViewModels.Model;

namespace ViewModels
{
    public class TabContentViewModel : ViewModel
    {
        private readonly IDispatcher _dispatcher;

        #region ChartKeys

        private static readonly string REAL = "Real";
        private static readonly string IMAGINARY = "Imaginary";
        private static readonly string MAGNITUDE = "Magnitude";
        private static readonly string PHASE = "Phase";

        #endregion

        #region Observable properties

        private TitledObject<IWave> _wave;
        public TitledObject<IWave> Wave 
        {
            get { return _wave; } 
            set
            {
                _wave = value;
                GenerateCharts(_wave.Value);
                OnPropertyChanged();
            }
        }

        private bool _isUsingRealAndImaginary = true;
        public bool IsUsingRealAndImaginary
        {
            get { return _isUsingRealAndImaginary; }
            set
            {
                _isUsingRealAndImaginary = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TopChart));
                OnPropertyChanged(nameof(BottomChart));
            }
        }

        public string FrequencyLabel => Wave.Value.FundamentalFrequencies.Any()
            ? string.Join("; ", Wave.Value.FundamentalFrequencies.Select(f => string.Format("{0:N2} Hz", f)))
            : string.Format("{0:N2} Hz", Wave.Value.Frequency);

        public ChartData TopChart => Charts[IsUsingRealAndImaginary ? REAL : MAGNITUDE];
        public ChartData BottomChart => Charts[IsUsingRealAndImaginary ? IMAGINARY : PHASE];

        #endregion

        public IDictionary<string, ChartData> Charts { get; } = new Dictionary<string, ChartData>();

        public TabContentViewModel() 
        {
            Name = nameof(TabContentViewModel);
        }

        public TabContentViewModel(string title) : this()
        {
            Wave = new TitledObject<IWave>(null, title);
        }

        public TabContentViewModel(string title, IWave wave) : this()
        {
            Wave = new TitledObject<IWave>(wave, title);
        }

        public TabContentViewModel(TitledObject<IWave> wave, IDispatcher dispatcher) : this()
        {
            _dispatcher = dispatcher;
            Wave = wave;
            wave.Value.PropertyChanged += (sender, args) => FrequencyChanged(args.PropertyName);
        }

        private void GenerateCharts(IWave wave) => 
            GenerateCharts(
                wave, 
                wave.Period.HasValue 
                    ? (int)(2 * Wave.Value.Period / Wave.Value.SamplePeriod) + 1 
                    : (int)wave.SampleRate / 4
            );

        private void GenerateCharts(IWave wave, int maxSamplesCount)
        {
            IEnumerable<double> horizontal = Enumerable
                .Range(0, maxSamplesCount)
                .Select(i => wave.IsComplex ? (double)(i * wave.SampleRate / maxSamplesCount) : (double)(i * wave.SamplePeriod));
            Charts[REAL] = new ChartData(horizontal, wave.Real.Take(maxSamplesCount), REAL);
            Charts[IMAGINARY] = new ChartData(horizontal, wave.Imaginary.Take(maxSamplesCount), IMAGINARY);
            Charts[MAGNITUDE] = new ChartData(horizontal, wave.Magnitude.Take(maxSamplesCount), MAGNITUDE);
            Charts[PHASE] = new ChartData(horizontal, wave.Phase.Take(maxSamplesCount), PHASE);
            IsUsingRealAndImaginary = !wave.IsComplex;
            OnPropertyChanged(nameof(TopChart));
            OnPropertyChanged(nameof(BottomChart));
        }

        private void FrequencyChanged(string property)
        {
            if (property.Equals(nameof(Wave.Value.Frequency)) 
                || property.Equals(nameof(Wave.Value.FundamentalFrequencies)))
            {
                _dispatcher.RunAsync(() =>
                {
                    GenerateCharts(Wave.Value, (int)(2 * Wave.Value.Period / Wave.Value.SamplePeriod) + 1);
                    OnPropertyChanged(nameof(FrequencyLabel));
                    return Task.CompletedTask;
                });
            }
        }
    }
}
