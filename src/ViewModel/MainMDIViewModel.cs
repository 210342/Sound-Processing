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
            IOController = controller;
        }

        public Task SaveCurrentSound()
        {
            return IOController?.SaveSoundFile();
        }

        public async Task OpenSoundFile()
        {
            TitledObject<IWave> wave = await IOController?.OpenSoundFile();
            TabContentViewModel newContent = new TabContentViewModel(wave);
            Contents.Add(newContent);
            SelectedTab = newContent;
        }

        public void CloseTab(object tab)
        {
             Contents.Remove(tab as TabContentViewModel);
        }
    }
}
