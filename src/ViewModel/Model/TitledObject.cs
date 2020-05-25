using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ViewModels.Model
{
    public class TitledObject<T> : INotifyPropertyChanged
    {
        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TitledObject(T value, string title)
        {
            Title = title;
            Value = value;
        }
    }
}
