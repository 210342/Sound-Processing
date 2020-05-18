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

        public ToolbarsViewModel(MainMDIViewModel mdi)
        {
            Name = nameof(ToolbarsViewModel);
            MainMDIViewModel = mdi;
            OpenCommand = new AsyncCommand(Open);
            SaveCommand = new AsyncCommand(Save);
        }

        public Task Open() => MainMDIViewModel?.OpenSoundFile();

        public Task Save() => MainMDIViewModel?.SaveCurrentSound();
    }
}
