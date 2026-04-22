using System;
using System.Collections.ObjectModel;

using Prism.Mvvm;

using Rating.WPF.Enums;
using Rating.WPF.Models;

namespace Rating.WPF.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


        public MainWindowViewModel()
        {
        }
    }
}