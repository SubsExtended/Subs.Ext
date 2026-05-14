// Subs.Ext\Tools\Rating.WPF\ViewModels\Dialogs\SettingsDialogViewModel.cs

using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Rating.WPF.Dialogs;
using Rating.WPF.Services;
using System;

namespace Rating.WPF.ViewModels.Dialogs
{
    public class SettingsDialogViewModel : BindableBase, IDialogAware
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        public event Action<IDialogResult> RequestClose;

        public SettingsDialogViewModel(ISettingsService settingsService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
        }

        private void BrowseVlc(string parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.Title = "Select VLC Media Player";

            if (dialog.ShowDialog() == true)
            {
                VlcPath = dialog.FileName;
            }
        }

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "ok")
            {
                _settingsService.Settings.VlcPath = VlcPath;
                _settingsService.Save();

                result = ButtonResult.OK;
            }
            else if (parameter?.ToLower() == "cancel")
            {
                result = ButtonResult.Cancel;
            }

            RaiseRequestClose(new DialogResult(result));
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {

        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            VlcPath = _settingsService.Settings.VlcPath;
        }

        private string _vlcPath;
        public string VlcPath
        {
            get { return _vlcPath; }
            set { SetProperty(ref _vlcPath, value); }
        }

        private string _title = "Settings";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        private DelegateCommand<string> _browseVlcCommand;
        public DelegateCommand<string> BrowseVlcCommand =>
            _browseVlcCommand ?? (_browseVlcCommand = new DelegateCommand<string>(BrowseVlc));

        public DelegateCommand OpenTempFilesCommand =>
            _openTempFiles ??= new DelegateCommand(() =>
            {
                _dialogService.ShowDialog(nameof(TempFilesDialog), null, null);
            });
        private DelegateCommand _openTempFiles;

    }
}