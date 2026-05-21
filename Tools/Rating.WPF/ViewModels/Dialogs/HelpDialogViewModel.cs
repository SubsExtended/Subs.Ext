// Subs.Ext\Tools\Rating.WPF\ViewModels\Dialogs\HelpDialogViewModel.cs

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Rating.WPF.General;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Rating.WPF.ViewModels.Dialogs
{
    public class HelpDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private string _title = "Help";
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
            //Message = parameters.GetValue<string>("message");

            Message = $"RatedSubtitles application(Windows) is a part of solution that will selectively display subtitles for audio / video media, depending on" +
                Environment.NewLine + Environment.NewLine +
                "a) how user rates his skill with the language of media." +
                Environment.NewLine +
                "b) how each phrase difficulty is rated in subtitles file." +
                Environment.NewLine + Environment.NewLine +
                "Phrases' difficulty can be rated depending on words frequency in the language, or how clearly the phrase is pronounced in the media." +
                Environment.NewLine +
                "Currently, the application works with SRT files only.Also, VLC media player must be installed on your pc." +
                Environment.NewLine + Environment.NewLine +
                "Run the application" +
                Environment.NewLine +
                "Click \"Settings\" button and set path to the VLC Player(usually its 'C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe')" +
                Environment.NewLine +
                "Click \"Open primary file\" and select a primary SRT file" +
                Environment.NewLine +
                "Optionally, click \"Open secondary file\" and add secondary SRT files" +
                Environment.NewLine +
                "After opening a primary SRT file - list of all subtitles will appear in the left pane" +
                Environment.NewLine +
                "Use \"Promote\" or \"Demote\" buttons to rate a subtitle line, rating A - hardest, E(or null) - easiest" +
                Environment.NewLine +
                "Click \"Open media file\" to open the relevant audio / video file" +
                Environment.NewLine +
                "Select your \"Language Level\" from the dropdown" +
                Environment.NewLine +
                "Click \"Play\" button (black triangle button)" +
                Environment.NewLine +
                "It will open the media with VLC player and display only the filtered subtitles";
        }
    }
}