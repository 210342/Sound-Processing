using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.DependencyInjection;

namespace SoundProcessing.DependencyInjection
{
    public class ViewModelLocator : IDisposable
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

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _kernel.Dispose();
                    Container = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
