using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class TabContentViewModel : ViewModel
    {
        #region Observable properties
        private IWave _wave;
        public IWave Wave 
        {
            get { return _wave; } 
            set
            {
                _wave = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public TabContentViewModel() 
        {
            Name = nameof(TabContentViewModel);
        }

        public TabContentViewModel(IWave wave) : this()
        {
            Wave = wave;
        }
    }
}
