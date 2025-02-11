﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace QuienEsQuien.Modelos
{
    public abstract class clsBase : INotifyPropertyChanged {
        public  event PropertyChangedEventHandler PropertyChanged;

        protected async virtual void NotifyPropertyChanged(string propertyName = null) {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
          });
           
        }
    }
}
