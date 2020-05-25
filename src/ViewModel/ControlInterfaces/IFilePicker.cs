using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ControlInterfaces
{
    public interface IFilePicker
    {
        string Path { get; }
        string Name { get; }
        Task<bool> OpenDialogAsync();
        Task<Stream> GetFileStreamAsync();
    }
}
