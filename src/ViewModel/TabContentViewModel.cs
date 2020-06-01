using SoundManipulation;
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

        private IEnumerable<Window> _windows = Enumerable.Empty<Window>();
        public IEnumerable<Window> Windows
        {
            get => _windows;
            set
            {
                _windows = value;
                OnPropertyChanged();
            }
        }

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
            IEnumerable<double> horizontal = wave.HorizontalAxis.Take(maxSamplesCount);
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
                    decimal windowLength = Wave.Value.SamplePeriod * WindowSize;
                    decimal period = Wave.Value.Period ?? (1 / Wave.Value.FundamentalFrequencies.Where(f => f.HasValue).FirstOrDefault() ?? 1);
                    Windows = Enumerable
                        .Range(0, Wave.Value.FundamentalFrequencies.Count())
                        .Zip(
                            Wave.Value.FundamentalFrequencies,
                            (i, f) => new Window(i * windowLength, (i + 1) * windowLength, f)
                        );
                    GenerateCharts(Wave.Value, (int)(2 * period / Wave.Value.SamplePeriod) + 1);
                    return Task.CompletedTask;
                });
            }
        }
    }
}
