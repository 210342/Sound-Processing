using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControlInterfaces;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;

namespace SoundProcessing.Adapters
{
    public class OpenSoundFilePicker : IOpenFilePicker
    {
        private readonly FileOpenPicker _picker;
        private StorageFile _file;

        public string Path => _file?.Path ?? string.Empty;
        public string Name => _file?.DisplayName ?? string.Empty;

        public OpenSoundFilePicker()
        {
            _picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            _picker.FileTypeFilter.Clear();
            _picker.FileTypeFilter.Add(".wav");
        }

        public async Task<bool> OpenDialogAsync()
        {
            _file = await _picker.PickSingleFileAsync();
            return _file != null && _file.IsAvailable;
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            if (_file is null)
            {
                return null;
            }
            return await _file.OpenStreamForReadAsync();
        }
    }
}
