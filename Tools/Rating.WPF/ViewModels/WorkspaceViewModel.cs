using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

using Prism.Commands;
using Prism.Mvvm;

using Rating.WPF.Enums;
using Rating.WPF.Models;
using Rating.WPF.Services;

namespace Rating.WPF.ViewModels
{
    public class WorkspaceViewModel : BindableBase
    {
        #region Fields

        private readonly IFileService fileService;

        #endregion

        #region CTOR

        public WorkspaceViewModel(IFileService fileService)
        {
            this.fileService = fileService;
            this.SecondaryFiles = new ObservableCollection<FileModel>();
        }

        #endregion

        #region Methods

        private async Task OpenFile(FileRank fileRank)
        {
            FileModel fileModel = new();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SRT files (*.srt)|*.srt";

            if (openFileDialog.ShowDialog() == true)
            {
                CancellationToken cancellationToken = new CancellationToken();
                string filePath = openFileDialog.FileName;
                fileModel = await fileService.ReadFileAsync(filePath, cancellationToken);
                fileModel.FileRank = fileRank;
            }
            else
            {
                return;
            }

            switch (fileRank)
            {
                case FileRank.Primary:

                    this.PrimaryFile = fileModel;
                    break;

                case FileRank.Secondary:

                    this.SecondaryFiles.Add(fileModel);
                    break;
            }
        }

        #endregion

        #region Properties

        private FileModel primaryFile;
        public FileModel PrimaryFile
        {
            get { return primaryFile; }
            set { SetProperty(ref primaryFile, value); }
        }

        private ObservableCollection<FileModel> secondaryFiles;
        public ObservableCollection<FileModel> SecondaryFiles
        {
            get { return secondaryFiles; }
            set { SetProperty(ref secondaryFiles, value); }
        }

        #endregion

        #region Commands

        private DelegateCommand<FileRank?> openFile;
        public DelegateCommand<FileRank?> OpenFileCommand =>
            openFile ?? (openFile = new DelegateCommand<FileRank?>(ExecuteOpenFileCommand));

        async void ExecuteOpenFileCommand(FileRank? parameter)
        {
            if (parameter.HasValue)
            {
                await OpenFile(parameter.Value);
            }
        }

        private DelegateCommand<object> promoteSubtitle;
        public DelegateCommand<object> PromoteSubtitleCommand =>
            promoteSubtitle ?? (promoteSubtitle = new DelegateCommand<object>(ExecutePromoteSubtitleCommand));

        void ExecutePromoteSubtitleCommand(object parameter)
        {

        }

        #endregion
    }
}
