using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ControlInterfaces
{
    public interface IFilePicker
    {
        Task<bool> OpenDialogAsync();
        Task<Stream> GetFileStreamAsync();
    }
}
