using AsyncAwaitBestPractices.MVVM;
using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
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
            if (wave != null)
            {
                SelectedTab = AddTab(wave);
                SelectedIndex = Contents.Count - 1;
            }
        }

        public void CloseTab(object tab)
        {
            Contents.Remove(tab as TabContentViewModel);
            SelectedIndex = Math.Min(SelectedIndex, Contents.Count - 1);
            SelectedTab = SelectedIndex < 0 ? null : Contents.ElementAt(SelectedIndex);
        }

        public async Task CalculatePeriodWithAMDF(double accuracy)
        {
            if (SelectedTab?.Wave?.Value is null)
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

        public async Task CalculatePeriodWithCepstralAnalysis(int windowSize)
        {
            if (SelectedTab?.Wave?.Value is null)
            {
                return;
            }

            SelectedTab.Wave.Value.Period = 1 /
                await Task.Run(() => SelectedTab.Wave.Value.CepstralAnalysis(windowSize));
            
            AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
            AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform.FourierTransform,
                $"{SelectedTab.Wave.Title} - Cepstrum"
            ));
        }

        public async Task CalculateFourierTransform()
        {
            if (SelectedTab?.Wave?.Value is null)
            {
                return;
            }

            IWave newWave = await Task.Run(() => SelectedTab.Wave.Value.FourierTransform);
            var newTab = AddTab(new TitledObject<IWave>(
                newWave,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
            SelectTab(newTab);
        }

        public async Task ShowSignalWithFrequency()
        {
            if (SelectedTab?.Wave?.Value?.Period is null)
            {
                return;
            }
            IWave current = SelectedTab.Wave.Value;
            IWave baseWave = await Task.Run(() => 
                WaveFactory.WaveWithFrequency(
                    Convert.ToDouble(current.Frequency ?? 1),
                    current.SamplePeriod,
                    (int)(Math.PI * 2 / (double)(current.SamplePeriod))
                )
            );
            var newTab = AddTab(new TitledObject<IWave>(baseWave, $"Basic wave {(int)current.Frequency} Hz"));
            SelectTab(newTab);
        }

        private TabContentViewModel AddTab(TitledObject<IWave> wave)
        {
            TabContentViewModel newContent = new TabContentViewModel(wave, _dispatcher);
            Contents.Add(newContent);
            return newContent;
        }

        private void SelectTab(TabContentViewModel tab)
        {
            SelectedTab = tab;
            SelectedIndex = Contents.IndexOf(tab);
        }
    }
}
