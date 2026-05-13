// Subs.Ext\Tools\Rating.WPF\Views\WorkspaceView.xaml.cs

using System;
using System.Windows.Controls;

namespace Rating.WPF.Views
{
    /// <summary>
    /// Interaction logic for FilesView.xaml
    /// </summary>
    public partial class WorkspaceView : UserControl
    {
        public WorkspaceView()
        {
            InitializeComponent();

            this.Unloaded += (s, e) =>
            {
                if (DataContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            };
        }
    }
}
