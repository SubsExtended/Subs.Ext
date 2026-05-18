// Subs.Ext\Tools\Rating.WPF\ViewModels\Dialogs\RunMediaPlayerDialogViewModel.cs

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.IO;

using Rating.WPF.General;

namespace Rating.WPF.ViewModels.Dialogs
{
    public class RunMediaPlayerDialogViewModel : BindableBase, IDialogAware
    {
        string _tempFileName;

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private string _title = "Play Media File";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "ok")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "cancel")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result, new DialogParameters { { "tempFileName", _tempFileName } }));
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
            string tempFolder = Path.GetTempPath();
            _tempFileName = Path.Combine(tempFolder, $"{Guid.NewGuid()}." + Constants.TempFilesSrtEnding);

            Message = "This will open the VLC Media Player.\r\n" +
                $"Make sure you have VLC installed and configured in {Constants.AppShortName} Settings.\r\n\r\n" +
                "Media file:\r\n" +
                parameters.GetValue<string>("mediaPath") + "\r\n\r\n" +
                "Subtitle file (will be created if you hit 'OK'):\r\n" +
                _tempFileName + "\r\n\r\n" +
                "Subtitle file will contain " +
                parameters.GetValue<int>("relevantSubCount") +
                " entries, filtered by your selected Language Level: " +
                parameters.GetValue<string>("myLanguageLevel") + "\r\n" +
                "It is based on the original subtitle file with your changes, but saved as a temporary file.\r\n\r\n" +
                "Your original subtitle file is:\r\n" +
                parameters.GetValue<string>("subtitlePath") + "\r\n" +
                "Your original subtitle file will remain unchanged.\r\n";
        }
    }
}