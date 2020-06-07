using AsyncAwaitBestPractices.MVVM;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ViewModels
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public string Name { get; protected set; } = nameof(ViewModel);

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = nameof(Name))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected internal void RaiseCanExecuteChanged()
        {
            foreach (var command in GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsAssignableFrom(typeof(AsyncCommand)))
                .Select(p => p.GetValue(this))
                .Cast<AsyncCommand>())
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }
}
