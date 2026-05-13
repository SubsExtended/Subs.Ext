// Subs.Ext\Tools\Rating.WPF\ViewModels\MainWindowViewModel.cs

using System.Reflection;
using Prism.Mvvm;
using Rating.WPF.General;

namespace Rating.WPF.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Title";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


        public MainWindowViewModel()
        {
            var info = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            Title = $"{Constants.AppShortName} - v{info.InformationalVersion}";
        }
    }
}