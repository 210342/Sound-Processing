using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Controllers
{
    public interface IIOSoundController
    {
        bool CanSaveSoundFile();
        Task SaveSoundFile();
        bool CanOpenSoundFile();
        Task<IWave> OpenSoundFile();
    }
}
