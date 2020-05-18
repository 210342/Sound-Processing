using SoundManipulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControlInterfaces;

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

        public bool CanSaveSoundFile() => _saveFileDialog != null;
        public async Task SaveSoundFile() { await Task.CompletedTask; }
        public bool CanOpenSoundFile() => _openFileDialog != null;
        public async Task<IWave> OpenSoundFile()
        {
            if (!CanOpenSoundFile())
            {
                return null;
            }

            if (!await _openFileDialog.OpenDialogAsync())
            {
                return null;
            }
            using (Stream stream = await _openFileDialog.GetFileStreamAsync())
            {
                return await Wave.ReadFromStreamAsync(stream);
            }
        }
    }
}
