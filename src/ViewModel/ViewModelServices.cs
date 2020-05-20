using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModels.Controllers;

namespace ViewModels
{
    public class ViewModelServices : NinjectModule
    {
        public override void Load()
        {
            Bind<IIOSoundController>().To<IOSoundController>();
            Bind<MainMDIViewModel>().To<MainMDIViewModel>().InSingletonScope();
            Bind<ToolbarsViewModel>().To<ToolbarsViewModel>().InSingletonScope();
            Bind<TabContentViewModel>().To<TabContentViewModel>().InTransientScope();
        }
    }
}
