using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.ControlInterfaces;
using ViewModels.Controllers;

namespace ViewModels
{
    public class ToolbarsViewModel : ViewModel
    {
        public MainMDIViewModel MainMDIViewModel { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AMDFCommand { get; }
        public ICommand CepstralCommand { get; }
        public ICommand FourierCommand { get; }

        public ToolbarsViewModel(MainMDIViewModel mdi)
        {
            Name = nameof(ToolbarsViewModel);
            MainMDIViewModel = mdi;
            OpenCommand = new AsyncCommand(Open);
            SaveCommand = new AsyncCommand(Save);
            AMDFCommand = new AsyncCommand(AMDF);
            CepstralCommand = new AsyncCommand(Cepstral);
            FourierCommand = new AsyncCommand(Fourier);
        }

        public Task Open() => MainMDIViewModel?.OpenSoundFile();

        public Task Save() => MainMDIViewModel?.SaveCurrentSound();

        public Task AMDF() => MainMDIViewModel?.CalculatePeriodWithAMDF();

        public Task Cepstral() => MainMDIViewModel?.CalculatePeriodWithCepstralAnalysis();

        public Task Fourier() => MainMDIViewModel?.CalculateFourierTransform();
    }
}
