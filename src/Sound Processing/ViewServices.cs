using Ninject.Modules;
using SoundProcessing.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControlInterfaces;

namespace SoundProcessing
{
    public class ViewServices : NinjectModule
    {
        public override void Load()
        {
            Bind<ISaveFilePicker>().To<SaveSoundFilePicker>();
            Bind<IOpenFilePicker>().To<OpenSoundFilePicker>();
        }
    }
}
