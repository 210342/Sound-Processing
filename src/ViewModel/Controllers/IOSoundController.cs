using SoundManipulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControlInterfaces;
using ViewModels.Model;

namespace ViewModels.Controllers
{
    public class IOSoundController : IIOSoundController
    {
        private readonly ISaveFilePicker _saveFileDialog;
        private readonly IOpenFilePicker _openFileDialog;

        public IOSoundController(ISaveFilePicker save, IOpenFilePicker open)
        {
            _saveFileDialog = save;
            _openFileDialog = open;
        }

        public bool CanSaveSoundFile(IWave wave) => _saveFileDialog != null && wave != null;

        public async Task SaveSoundFile(IWave wave)
        {
            if (!CanSaveSoundFile(wave)
                || !await _saveFileDialog.OpenDialogAsync())
            {
                return;
            }

            using (Stream stream = await _saveFileDialog.GetFileStreamAsync())
            {
                await wave.WriteToStreamAsync(stream);
            }
        }

        public bool CanOpenSoundFile() => _openFileDialog != null;

        public async Task<TitledObject<IWave>> OpenSoundFile()
        {
            if (!CanOpenSoundFile()
                || !await _openFileDialog.OpenDialogAsync())
            {
                return null;
            }

            using (Stream stream = await _openFileDialog.GetFileStreamAsync())
            {
                return new TitledObject<IWave>(await Wave.ReadFromStreamAsync(stream), _openFileDialog.Name);
            }
        }
    }
}
