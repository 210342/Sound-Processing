using System;
using System.Threading.Tasks;

namespace ViewModels.DependencyInjection
{
    public interface IDispatcher
    {
        Task RunAsync(Func<Task> func);
    }
}
