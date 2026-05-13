// Subs.Ext\Tools\Rating.WPF\ViewModels\Dialogs\TempFilesDialogViewModel.cs

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Rating.WPF.General;

namespace Rating.WPF.ViewModels.Dialogs
{
    public class TempFilesDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Temporary Subtitle Files";

        public event Action<IDialogResult> RequestClose;

        public ObservableCollection<TempFileInfo> TempFiles { get; } = new();

        public TempFilesDialogViewModel()
        {
            LoadFiles();
        }

        private void LoadFiles()
        {
            TempFiles.Clear();

            string temp = Path.GetTempPath();
            var files = Directory.GetFiles(temp, $"*.{Constants.TempFilesSrtEnding}");

            foreach (var f in files)
            {
                var info = new FileInfo(f);
                TempFiles.Add(new TempFileInfo
                {
                    FullPath = f,
                    FileName = info.Name,
                    Created = info.CreationTime,
                    SizeKb = info.Length / 1024
                });
            }
        }

        public DelegateCommand DeleteSelectedCommand =>
            _deleteSelected ??= new DelegateCommand(() =>
            {
                foreach (var file in TempFiles.Where(f => f.IsSelected).ToList())
                {
                    TryDelete(file.FullPath);
                    TempFiles.Remove(file);
                }
            });
        private DelegateCommand _deleteSelected;

        public DelegateCommand DeleteAllCommand =>
            _deleteAll ??= new DelegateCommand(() =>
            {
                foreach (var file in TempFiles.ToList())
                {
                    TryDelete(file.FullPath);
                    TempFiles.Remove(file);
                }
            });
        private DelegateCommand _deleteAll;

        public DelegateCommand OpenFolderCommand =>
            _openFolder ??= new DelegateCommand(() =>
            {
                Process.Start("explorer.exe", Path.GetTempPath());
            });
        private DelegateCommand _openFolder;

        public DelegateCommand<string> CloseDialogCommand =>
            _close ??= new DelegateCommand<string>(param =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            });
        private DelegateCommand<string> _close;

        private void TryDelete(string path)
        {
            try { File.Delete(path); }
            catch { /* ignore locked files */ }
        }

        public void OnDialogOpened(IDialogParameters parameters) { }
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
    }

    public class TempFileInfo : BindableBase
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public DateTime Created { get; set; }
        public long SizeKb { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}