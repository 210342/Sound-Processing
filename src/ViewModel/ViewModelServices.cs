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
            foreach (Type viewModelType in GetType().Assembly.GetTypes()
                .Where(t => typeof(ViewModel).IsAssignableFrom(t) && !t.Equals(typeof(ViewModel))))
            {
                Bind(viewModelType).To(viewModelType);
            }
        }
    }
}
