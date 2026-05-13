// Subs.Ext\Tools\Rating.WPF\Dialogs\HelpDialog.xaml.cs

using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Rating.WPF.Dialogs
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : UserControl
    {
        public HelpDialog()
        {
            InitializeComponent();
        }

        private void HyperlinkEmail_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void HyperlinkGitHub_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
