using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Controllers;

namespace ViewModels
{
    public class MainMDIViewModel : ViewModel
    {
        #region Observable properties
        public ObservableCollection<TabContentViewModel> Contents { get; } = new ObservableCollection<TabContentViewModel>();
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
            IWave wave = await IOController?.OpenSoundFile();
            Contents.Add(new TabContentViewModel(wave));
        }
    }
}
