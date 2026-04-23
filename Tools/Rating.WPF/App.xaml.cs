using System.Windows;

using Prism.Ioc;
using Prism.Regions;

using Rating.WPF.General;
using Rating.WPF.Services;
using Rating.WPF.Views;

namespace Rating.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IRegionManager _regionManager;

        protected override Window CreateShell()
        {
            // Return the main window (shell). Do not navigate here because regions are
            // not guaranteed to be available until after initialization.
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register the view for navigation so RequestNavigate can use the view's name
            containerRegistry.RegisterForNavigation<WorkspaceView, ViewModels.WorkspaceViewModel>(nameof(WorkspaceView));

            containerRegistry.RegisterSingleton<IFileService, FileService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Now that the shell (MainWindow) is created and regions are available,
            // request navigation to the FilesView into the ContentRegion.
            _regionManager = Container.Resolve<IRegionManager>();
            _regionManager.RequestNavigate(Constants.ContentRegion, nameof(WorkspaceView));
        }
    }
}
