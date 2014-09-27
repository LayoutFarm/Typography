using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sample.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
