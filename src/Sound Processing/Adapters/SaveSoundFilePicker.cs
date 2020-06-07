using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControlInterfaces;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SoundProcessing.Adapters
{
    class SaveSoundFilePicker : ISaveFilePicker
    {
        private readonly FileSavePicker _picker;
        private StorageFile _file;

        public string Path => _file?.Path ?? string.Empty;
        public string Name => _file?.DisplayName ?? string.Empty;

        public SaveSoundFilePicker()
        {
            _picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            _picker.FileTypeChoices.Add("Wave", new List<string>() { ".wav" });
        }

        public async Task<bool> OpenDialogAsync()
        {
            _file = await _picker.PickSaveFileAsync();
            return _file?.IsAvailable ?? false;
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            if (_file is null)
            {
                return null;
            }
            return await _file.OpenStreamForWriteAsync();
        }
    }
}
