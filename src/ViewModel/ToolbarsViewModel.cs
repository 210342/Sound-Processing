using AsyncAwaitBestPractices.MVVM;
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
        }

        public Task Open() => MainMDIViewModel?.OpenSoundFile();

        public Task Save() => MainMDIViewModel?.SaveCurrentSound();

        public Task AMDF() => MainMDIViewModel?.CalculatePeriodWithAMDF(Accuracy);

        public Task Cepstral() => MainMDIViewModel?.CalculatePeriodWithCepstralAnalysis();

        public Task Fourier() => MainMDIViewModel?.CalculateFourierTransform();

        private async Task Job(Func<Task> job)
        {
            _jobs.Enqueue(true);
            OnPropertyChanged(nameof(IsCalculating));
            await job();
            _jobs.Dequeue();
            OnPropertyChanged(nameof(IsCalculating));
        }
    }
}
