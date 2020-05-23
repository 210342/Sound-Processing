using AsyncAwaitBestPractices.MVVM;
using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.Controllers;
using ViewModels.DependencyInjection;
using ViewModels.Model;

namespace ViewModels
{
    public class MainMDIViewModel : ViewModel
    {
        private readonly IDispatcher _dispatcher;

        #region Observable properties
        public ObservableCollection<TabContentViewModel> Contents { get; } = new ObservableCollection<TabContentViewModel>();

        private TabContentViewModel _selectedTab;

        public TabContentViewModel SelectedTab 
        { 
            get { return _selectedTab; } 
            set
            { 
                _selectedTab = value; 
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region Commands

        #endregion

        public IIOSoundController IOController { get; }

        public MainMDIViewModel(IIOSoundController controller, IDispatcher dispatcher)
        {
            Name = nameof(MainMDIViewModel);
            _dispatcher = dispatcher;
            IOController = controller;
        }

        public Task SaveCurrentSound()
        {
            return IOController?.SaveSoundFile();
        }

        public async Task OpenSoundFile()
        {
            TitledObject<IWave> wave = await IOController?.OpenSoundFile();
            SelectedTab = AddTab(wave);
        }

        public void CloseTab(object tab)
        {
             Contents.Remove(tab as TabContentViewModel);
        }

        public async Task CalculatePeriodWithAMDF(double accuracy)
        {
            if (SelectedTab is null)
            {
                return;
            }

            decimal? period =
                await Task.Run(() => SelectedTab?.Wave?.Value?.AMDF(accuracy));

            if (period.HasValue)
            {
                SelectedTab.Wave.Value.Period = period;
            }
        }

        public async Task CalculatePeriodWithCepstralAnalysis() 
        {
            if (SelectedTab is null)
            {
                return;
            }

            SelectedTab.Wave.Value.Period = 
                await Task.Run(() => SelectedTab.Wave.Value.CepstralAnalysis());
            
            AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
            AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier - Fourier"
            ));
        }

        public async Task CalculateFourierTransform()
        {
            if (SelectedTab is null)
            {
                return;
            }

            IWave newWave = await Task.Run(() => SelectedTab.Wave.Value.FourierTransform);
            SelectedTab = AddTab(new TitledObject<IWave>(
                newWave,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
        }

        private TabContentViewModel AddTab(TitledObject<IWave> wave)
        {
            TabContentViewModel newContent = new TabContentViewModel(wave, _dispatcher);
            Contents.Add(newContent);
            return newContent;
        }
    }
}
