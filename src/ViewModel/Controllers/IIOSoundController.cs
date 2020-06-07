using SoundManipulation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Model;

namespace ViewModels.Controllers
{
    public interface IIOSoundController
    {
        bool CanSaveSoundFile(IWave wave);
        Task SaveSoundFile(IWave wave);
        bool CanOpenSoundFile();
        Task<TitledObject<IWave>> OpenSoundFile();
    }
}
