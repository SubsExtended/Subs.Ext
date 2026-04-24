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
            this.SecondaryFileCollection = new ObservableCollection<FileModel>();
        }

        #endregion

        #region Methods

        private async Task OpenFile(FileRankEnum fileRank)
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
                case FileRankEnum.Primary:

                    this.PrimaryFile = fileModel;
                    break;

                case FileRankEnum.Secondary:

                    this.SecondaryFileCollection.Add(fileModel);
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

        private ObservableCollection<FileModel> secondaryFileCollection;
        public ObservableCollection<FileModel> SecondaryFileCollection
        {
            get { return secondaryFileCollection; }
            set { SetProperty(ref secondaryFileCollection, value); }
        }

        private SubtitleModel subtitleSelectedItem;
        public SubtitleModel SubtitleSelectedItem
        {
            get { return subtitleSelectedItem; }
            set { SetProperty(ref subtitleSelectedItem, value); }
        }

        #endregion

        #region Commands

        private DelegateCommand<FileRankEnum?> openFile;
        public DelegateCommand<FileRankEnum?> OpenFileCommand =>
            openFile ?? (openFile = new DelegateCommand<FileRankEnum?>(ExecuteOpenFileCommand));

        async void ExecuteOpenFileCommand(FileRankEnum? parameter)
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

        private DelegateCommand<object> demoteSubtitle;
        public DelegateCommand<object> DemoteSubtitleCommand =>
            demoteSubtitle ?? (demoteSubtitle = new DelegateCommand<object>(ExecuteDemoteSubtitleCommand));

        void ExecuteDemoteSubtitleCommand(object parameter)
        {

        }

        #endregion
    }
}
