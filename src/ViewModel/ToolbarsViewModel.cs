using AsyncAwaitBestPractices.MVVM;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.ControlInterfaces;
using ViewModels.Controllers;
using ViewModels.DependencyInjection;

namespace ViewModels
{
    public class ToolbarsViewModel : ViewModel
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

        #endregion

        public ToolbarsViewModel(MainMDIViewModel mdi, IDispatcher dispatcher)
        {
            Name = nameof(ToolbarsViewModel);
            MainMDIViewModel = mdi;
            _dispatcher = dispatcher;
            OpenCommand = new AsyncCommand(() => Job(Open));
            SaveCommand = new AsyncCommand(() => Job(Save));
            AMDFCommand = new AsyncCommand(() => Job(AMDF));
            CepstralCommand = new AsyncCommand(() => Job(Cepstral));
            FourierCommand = new AsyncCommand(() => Job(Fourier));
            ShowBaseFrequencySignalCommand = new AsyncCommand(() => Job(ShowBaseFrequencySignal));
        }

        public Task Open() => MainMDIViewModel?.OpenSoundFile();

        public Task Save() => MainMDIViewModel?.SaveCurrentSound();

        public Task AMDF() => MainMDIViewModel?.CalculatePeriodWithAMDF(Accuracy);

        public Task Cepstral() => MainMDIViewModel?.CalculatePeriodWithCepstralAnalysis(GetSelectedWindowSize());

        public Task Fourier() => MainMDIViewModel?.CalculateFourierTransform();

        public Task ShowBaseFrequencySignal() => MainMDIViewModel?.ShowSignalWithFrequency();

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
    }
}
