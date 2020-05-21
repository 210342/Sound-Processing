using AsyncAwaitBestPractices.MVVM;
using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.Controllers;
using ViewModels.Model;

namespace ViewModels
{
    public class MainMDIViewModel : ViewModel
    {
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

        public MainMDIViewModel(IIOSoundController controller)
        {
            Name = nameof(MainMDIViewModel);
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

        public async Task CalculatePeriodWithAMDF() => 
            await Task.Run(() => SelectedTab?.Wave?.Value?.AMDF());

        public async Task CalculatePeriodWithCepstralAnalysis() => await Task.Run(() =>
        {
            if (SelectedTab is null)
            {
                return;
            }

            SelectedTab.Wave.Value.CepstralAnalysis();
            AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
            AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier - Fourier"
            ));
        });

        public async Task CalculateFourierTransform() => await Task.Run(() =>
        {
            if (SelectedTab is null)
            {
                return;
            }

            SelectedTab = AddTab(new TitledObject<IWave>(
                SelectedTab.Wave.Value.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
        }
        );

        private TabContentViewModel AddTab(TitledObject<IWave> wave)
        {
            TabContentViewModel newContent = new TabContentViewModel(wave);
            Contents.Add(newContent);
            return newContent;
        }
    }
}
