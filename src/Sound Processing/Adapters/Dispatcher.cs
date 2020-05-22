using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.DependencyInjection;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SoundProcessing.Adapters
{
    public class Dispatcher : IDispatcher
    {
        private CoreDispatcher _dispatcher;

        public Dispatcher()
        {
            _dispatcher = Window.Current.Dispatcher;
        }

        public async Task RunAsync(Func<Task> func)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => func());
        }
    }
}
