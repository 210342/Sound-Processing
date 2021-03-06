﻿using AsyncAwaitBestPractices.MVVM;
using NAudio.Wave;
using SoundManipulation.Filtering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.ControlInterfaces;
using ViewModels.Controllers;
using ViewModels.DependencyInjection;

namespace ViewModels
{
    public class ToolbarsViewModel : ViewModel, IDisposable
    {
        private readonly IDispatcher _dispatcher;
        private readonly Queue<bool> _jobs = new Queue<bool>();

        public MainMDIViewModel MainMDIViewModel { get; }

        #region Commands

        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AMDFCommand { get; }
        public ICommand CepstralCommand { get; }
        public ICommand FourierCommand { get; }
        public ICommand ShowBaseFrequencySignalCommand { get; }
        public ICommand FilterCommand { get; }

        #endregion 

        #region Observable properties

        public bool IsCalculating => _jobs.Count > 0;

        private double _accuracy = 0.015;

        public double Accuracy
        {
            get { return _accuracy; }
            set
            {
                _accuracy = value;
                OnPropertyChanged();
            }
        }

        #region Fourier

        public string WindowSizeLabel => $"Window size: {GetSelectedWindowSize()}";

        private bool _isWindow512Checked = false;
        public bool IsWindow512Checked 
        {
            get => _isWindow512Checked;
            set
            {
                _isWindow512Checked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowSizeLabel));
            }
        }

        private bool _isWindow1024Checked = false;
        public bool IsWindow1024Checked
        {
            get => _isWindow1024Checked;
            set
            {
                _isWindow1024Checked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowSizeLabel));
            }
        }

        private bool _isWindow2048Checked = true;
        public bool IsWindow2048Checked
        {
            get => _isWindow2048Checked;
            set
            {
                _isWindow2048Checked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowSizeLabel));
            }
        }

        private bool _isWindow4096Checked = false;
        public bool IsWindow4096Checked
        {
            get => _isWindow4096Checked;
            set
            {
                _isWindow4096Checked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowSizeLabel));
            }
        }

        #endregion Fourier

        #region Filter

        private bool _isUsingRectangularWindow = true;
        public bool IsUsingRectangularWindow
        {
            get => _isUsingRectangularWindow;
            set
            {
                _isUsingRectangularWindow = value;
                OnPropertyChanged();
            }
        }

        private bool _isUsingHannWindow = false;
        public bool IsUsingHannWindow
        {
            get => _isUsingHannWindow;
            set
            {
                _isUsingHannWindow = value;
                OnPropertyChanged();
            }
        }

        private bool _isUsingHammingWindow = false;
        public bool IsUsingHammingWindow
        {
            get => _isUsingHammingWindow;
            set
            {
                _isUsingHammingWindow = value;
                OnPropertyChanged();
            }
        }

        private bool _isUsingLowPass = true;
        public bool IsUsingLowPass
        {
            get => _isUsingLowPass;
            set
            {
                _isUsingLowPass = value;
                OnPropertyChanged();
            }
        }

        private bool _isUsingMiddlePass = false;
        public bool IsUsingMiddlePass
        {
            get => _isUsingMiddlePass;
            set
            {
                _isUsingMiddlePass = value;
                OnPropertyChanged();
            }
        }

        private bool _isUsingHighPass = false;
        public bool IsUsingHighPass
        {
            get => _isUsingHighPass;
            set
            {
                _isUsingHighPass = value;
                OnPropertyChanged();
            }
        }

        private bool _isUsingCausalFilter = true;
        public bool IsUsingCausalFilter
        {
            get => _isUsingCausalFilter;
            set
            {
                _isUsingCausalFilter = value;
                OnPropertyChanged();
            }
        }

        private int _windowLength = 1024;
        public double WindowLength
        {
            get => _windowLength;
            set
            {
                _windowLength = (int)value;
                OnPropertyChanged();
            }
        }

        private int _filterLength = 33;
        public double FilterLength
        {
            get => _filterLength;
            set
            {
                _filterLength = (int)value;
                OnPropertyChanged();
            }
        }

        private int _hopSize = 512;
        public double HopSize
        {
            get => _hopSize;
            set
            {
                _hopSize = (int)value;
                OnPropertyChanged();
            }
        }

        private int _cutoffFrequency = 400;

        public double CutoffFrequency
        {
            get => _cutoffFrequency;
            set
            {
                _cutoffFrequency = (int)value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        public ToolbarsViewModel(MainMDIViewModel mdi, IDispatcher dispatcher)
        {
            Name = nameof(ToolbarsViewModel);
            MainMDIViewModel = mdi;
            mdi.PropertyChanged += OnMdiPropertyChanged;
            _dispatcher = dispatcher;
            OpenCommand = new AsyncCommand(() => Job(Open));
            SaveCommand = new AsyncCommand(() => Job(Save), o => CanSave());
            AMDFCommand = new AsyncCommand(() => Job(AMDF));
            CepstralCommand = new AsyncCommand(() => Job(Cepstral));
            FourierCommand = new AsyncCommand(() => Job(Fourier));
            ShowBaseFrequencySignalCommand = new AsyncCommand(() => Job(ShowBaseFrequencySignal));
            FilterCommand = new AsyncCommand(() => Job(Filter));
        }

        #region Commands

        public Task Open() => MainMDIViewModel?.OpenSoundFile();

        public bool CanSave() => MainMDIViewModel?.CanSaveCurrentSound() ?? false;
        public Task Save() => MainMDIViewModel?.SaveCurrentSound();

        public Task AMDF() => MainMDIViewModel?.CalculatePeriodWithAMDF(GetSelectedWindowSize(), Accuracy);

        public Task Cepstral() => MainMDIViewModel?.CalculatePeriodWithCepstralAnalysis(GetSelectedWindowSize());

        public Task Fourier() => MainMDIViewModel?.CalculateFourierTransform();

        public Task ShowBaseFrequencySignal() => MainMDIViewModel?.ShowSignalWithFundamentalFrequencies(GetSelectedWindowSize());

        public Task Filter() => MainMDIViewModel?.FilterSignal(GetFilterType(), GetWindow(), _hopSize);

        #endregion

        private async Task Job(Func<Task> job)
        {
            _jobs.Enqueue(true);
            OnPropertyChanged(nameof(IsCalculating));
            await job();
            _jobs.Dequeue();
            OnPropertyChanged(nameof(IsCalculating));
        }

        private int GetSelectedWindowSize()
        {
            if (IsWindow512Checked)
            {
                return 512;
            }
            else if (IsWindow1024Checked)
            {
                return 1024;
            }
            else if (IsWindow2048Checked)
            {
                return 2048;
            }
            else
            {
                return 4096;
            }
        }

        private WaveWindow GetWindow()
        {
            if (IsUsingHammingWindow)
            {
                return new WaveWindow(WaveWindow.Hamming, _windowLength);
            }
            else if(IsUsingHannWindow)
            {
                return new WaveWindow(WaveWindow.VonHann, _windowLength);
            }
            else
            {
                return new WaveWindow(WaveWindow.Rectangular, _windowLength);
            }
        }

        private IFilter GetFilterType()
        {
            if (IsUsingLowPass)
            {
                return new LowPassFilter(_filterLength, _cutoffFrequency, IsUsingCausalFilter);
            }
            else if (IsUsingHighPass)
            {
                return new HighPassFilter(_filterLength, _cutoffFrequency, IsUsingCausalFilter);
            }
            else
            {
                return new MiddlePassFilter(_filterLength, _cutoffFrequency, IsUsingCausalFilter);
            }
        }

        private void OnMdiPropertyChanged(object sender, PropertyChangedEventArgs args)
            => RaiseCanExecuteChanged();

        #region IDisposable

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MainMDIViewModel.PropertyChanged -= OnMdiPropertyChanged;
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
