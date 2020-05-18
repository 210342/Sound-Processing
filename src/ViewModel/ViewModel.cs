using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    }
}
