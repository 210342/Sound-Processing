using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModels.Model;

namespace ViewModels
{
    public class TabContentViewModel : ViewModel
    {
        #region ChartKeys

        private static readonly string REAL = "Real";
        private static readonly string IMAGINARY = "Imag";
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
                OnPropertyChanged();
            }
        }

        private bool _isUsingAmplitudeAndPhase = false;
        public bool IsUsingAmplitudeAndPhase
        {
            get { return _isUsingAmplitudeAndPhase; }
            set
            {
                _isUsingAmplitudeAndPhase = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TopChart));
                OnPropertyChanged(nameof(BottomChart));
            }
        }

        public IDictionary<string, ChartData> Charts { get; } = new Dictionary<string, ChartData>();

        public ChartData TopChart => Charts[IsUsingAmplitudeAndPhase ? MAGNITUDE : REAL];
        public ChartData BottomChart => Charts[IsUsingAmplitudeAndPhase ? PHASE : IMAGINARY];

        #endregion

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

        public TabContentViewModel(TitledObject<IWave> wave) : this()
        {
            Wave = wave;
        }

        private void GenerateCharts(IWave wave)
        {
            IEnumerable<double> horizontal = Enumerable
                .Range(0, wave.SamplesCount)
                .Select(i => i * decimal.ToDouble(wave.SamplePeriod));
            Charts.Add(REAL, new ChartData(horizontal, wave.Real));
            Charts.Add(IMAGINARY, new ChartData(horizontal, wave.Imaginary));
            Charts.Add(MAGNITUDE, new ChartData(horizontal, wave.Magnitude));
            Charts.Add(PHASE, new ChartData(horizontal, wave.Phase));
        }
    }
}
