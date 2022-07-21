using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MalApi.Models
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string property = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string property = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value) == false)
            {
                storage = value;
                RaisePropertyChanged(property);
            }
        }
    }
}
