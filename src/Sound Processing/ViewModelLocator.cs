using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace SoundProcessing
{
    public class ViewModelLocator
    {
        public static Lazy<ViewModelLocator> Container { get; set; } = 
            new Lazy<ViewModelLocator>(() => new ViewModelLocator(), true);

        private readonly IKernel _kernel;

        private ViewModelLocator()
        {
            _kernel = new StandardKernel();
            _kernel.Load(new INinjectModule[] { new ViewServices(), new ViewModelServices() });
        }

        public T GetViewModel<T>() where T : ViewModel
        {
            return _kernel.Get<T>();
        }
    }
}
