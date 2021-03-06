﻿using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.DependencyInjection;
using ViewModels.Model;

namespace ViewModels
{
    public class TabContentViewModel : ViewModel, IDisposable
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

        private int _windowSize;
        public int WindowSize
        {
            get => _windowSize;
            set
            {
                _windowSize = value;
                OnPropertyChanged();
            }
        }

        private ICollection<Window> _windows = new List<Window>();

        public ICollection<Window> Windows
        {
            get => _windows;
            set
            {
                _windows = value;
                OnPropertyChanged();
            }
        }

        private int? _selectedWindowIndex = null;
        public int? SelectedWindowIndex
        {
            get => _selectedWindowIndex;
            set
            {
                _selectedWindowIndex = value;
                OnPropertyChanged();
            }
        }

        public ChartData TopChart => Charts[IsUsingRealAndImaginary ? REAL : MAGNITUDE];
        public ChartData BottomChart => Charts[IsUsingRealAndImaginary ? IMAGINARY : PHASE];

        #endregion

        #region Properties

        public uint ChartsUnitStep { get; set; } = 8;
        public IDictionary<string, ChartData> Charts { get; } = new Dictionary<string, ChartData>();

        #endregion

        #region Constructors

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
            wave.Value.PropertyChanged += OnWavePropertyChanged;
        }

        #endregion

        public void GenerateCharts(int? maxSamplesCount, uint unitStep) =>
            GenerateCharts(Wave.Value, maxSamplesCount ?? Wave.Value.SamplesCount, unitStep);

        private void GenerateCharts(IWave wave, int maxSamplesCount, uint unitStep)
        {
            IEnumerable<double> horizontal = wave.HorizontalAxis.Where((sample, index) => index % unitStep == 0).Take(maxSamplesCount);
            Charts[REAL] = new ChartData(horizontal, wave.Real.Where((sample, index) => index % unitStep == 0).Take(maxSamplesCount), REAL);
            Charts[IMAGINARY] = new ChartData(horizontal, wave.Imaginary.Where((sample, index) => index % unitStep == 0).Take(maxSamplesCount), IMAGINARY);
            Charts[MAGNITUDE] = new ChartData(horizontal, wave.Magnitude.Where((sample, index) => index % unitStep == 0).Take(maxSamplesCount), MAGNITUDE);
            Charts[PHASE] = new ChartData(horizontal, wave.Phase.Where((sample, index) => index % unitStep == 0).Take(maxSamplesCount), PHASE);
            IsUsingRealAndImaginary = !wave.IsComplex;
            OnPropertyChanged(nameof(TopChart));
            OnPropertyChanged(nameof(BottomChart));
        }

        private void GenerateCharts(IWave wave) =>
            GenerateCharts(
                wave,
                wave.Period.HasValue
                    ? (int)(2 * Wave.Value.Period / Wave.Value.SamplePeriod) + 1
                    : (int)wave.SampleRate / 4,
                ChartsUnitStep
            );


        private void OnWavePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            string property = args.PropertyName;
            if (property.Equals(nameof(Wave.Value.FundamentalFrequencies)))
            {
                _dispatcher.RunAsync(() =>
                {
                    decimal windowLength = Wave.Value.SamplePeriod * WindowSize;
                    decimal period = Wave.Value.Period ?? 1 / Wave.Value.FundamentalFrequencies.Where(f => f.HasValue).FirstOrDefault() ?? 1;
                    Windows = Wave.Value.FundamentalFrequencies.Select((f, i) => 
                        new Window((i * WindowSize, (i + 1) * WindowSize), i * windowLength, (i + 1) * windowLength, f)
                    ).ToList();
                    return Task.CompletedTask;
                });
            }
        }

        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Wave.PropertyChanged -= OnWavePropertyChanged;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
