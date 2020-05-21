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

        public ChartData TopChart => Charts[IsUsingRealAndImaginary ? REAL : MAGNITUDE];
        public ChartData BottomChart => Charts[IsUsingRealAndImaginary ? IMAGINARY : PHASE];

        #endregion

        public IDictionary<string, ChartData> Charts { get; } = new Dictionary<string, ChartData>();
        public IEnumerable<double> Tmp { get; } = new double[] { 0, 2, 1, 8, 6, 4, 5 };

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
                .Select(i => (double)i);
            Charts.Add(REAL, new ChartData(horizontal, wave.Real));
            Charts.Add(IMAGINARY, new ChartData(horizontal, wave.Imaginary));
            Charts.Add(MAGNITUDE, new ChartData(horizontal, wave.Magnitude));
            Charts.Add(PHASE, new ChartData(horizontal, wave.Phase));
        }
    }
}
