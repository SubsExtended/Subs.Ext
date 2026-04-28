// Subs.Ext\Tools\Rating.WPF\ViewModels\WorkspaceViewModel.cs

using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Rating.WPF.Dialogs;
using Rating.WPF.Enums;
using Rating.WPF.Models;
using Rating.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Rating.WPF.ViewModels
{
    public class WorkspaceViewModel : BindableBase
    {
        #region Fields

        private readonly IFileService _fileService;

        private IDialogService _dialogService;

        #endregion

        #region CTOR

        public WorkspaceViewModel(IFileService fileService, IDialogService dialogService)
        {
            this._fileService = fileService;
            this._dialogService = dialogService;
        }

        #endregion

        #region Methods

        private async Task OpenFile(FileRankEnum fileRank)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SRT files (*.srt)|*.srt";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }

            var fileModel = await _fileService.ReadFileAsync(openFileDialog.FileName, new CancellationToken());
            fileModel.FileRank = fileRank;

            if (fileRank == FileRankEnum.Primary)
            {
                // Replace existing primary if it exists
                var existing = FilesCollection.FirstOrDefault(f => f.FileRank == FileRankEnum.Primary);
                if (existing != null) FilesCollection.Remove(existing);
            }

            FilesCollection.Add(fileModel);

            // Refresh the filtered helper properties
            RaisePropertyChanged(nameof(PrimaryFile));
            RaisePropertyChanged(nameof(SecondaryFiles));

            // Initial selection sync
            if (fileRank == FileRankEnum.Primary)
            {
                PrimaryFileSubtitlesSelectedItem = fileModel.SubtitleCollection.FirstOrDefault();
            }
            else if (PrimaryFileSubtitlesSelectedItem != null)
            {
                // If we just added a secondary, sync its selection to the current primary position
                SyncSecondarySelections(PrimaryFileSubtitlesSelectedItem.Position);
            }
        }

        private void SyncSecondarySelections(int position)
        {
            foreach (var file in SecondaryFiles)
            {
                file.SubtitleSelectedItem = file.SubtitleCollection.FirstOrDefault(s => s.Position == position);
            }
        }

        private void ApplyRating(SubtitleModel subtitleModel, SubtitleRatingEnum? newRating)
        {
            // User can set ratings for secondary file subtitles independently, but they won't sync to the primary. Only changes to the primary sync to secondaries.
            var fileWithSubtitle = FilesCollection.FirstOrDefault(f => f.SubtitleCollection.Any(s => s.PK == subtitleModel.PK));
            if (fileWithSubtitle == null) return;

            // Always update the specific item clicked
            subtitleModel.RatingCurrent = newRating;

            // If it was a Primary sub, sync all other files at this position
            if (fileWithSubtitle.FileRank == FileRankEnum.Primary)
            {
                foreach (var file in FilesCollection)
                {
                    // Skip the file we already updated
                    if (file == fileWithSubtitle) continue;

                    var sub = file.SubtitleCollection.FirstOrDefault(s => s.Position == subtitleModel.Position);
                    if (sub != null)
                    {
                        sub.RatingCurrent = newRating;
                    }
                }
            }

            foreach (var file in FilesCollection)
            {
                _ = file.SetIsDirty();
            }
        }

        string Title = "Rating Tool";
        private void ShowDialog()
        {
            var message = "This is a message that should be shown in the dialog.";
            //using the dialog service as-is
            _dialogService.ShowDialog(nameof(YesNoDialog), new DialogParameters($"message={message}"), r =>
            {
                if (r.Result == ButtonResult.None)
                    Title = "Result is None";
                else if (r.Result == ButtonResult.OK)
                    Title = "Result is OK";
                else if (r.Result == ButtonResult.Cancel)
                    Title = "Result is Cancel";
                else
                    Title = "I Don't know what you did!?";
            });
        }

        private void DoFileOperation(FileOperationEnum operation)
        {
            switch (operation)
            {
                case FileOperationEnum.PrimarySave:
                    // Implement save logic
                    break;
                case FileOperationEnum.PrimarySaveAs:
                    // Implement save as logic
                    break;
                case FileOperationEnum.PrimaryClose:
                    // Implement close logic
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Properties

        // UI Bindings for the two panels
        public FileModel PrimaryFile => FilesCollection.FirstOrDefault(f => f.FileRank == FileRankEnum.Primary);
        public IEnumerable<FileModel> SecondaryFiles => FilesCollection.Where(f => f.FileRank == FileRankEnum.Secondary);

        private ObservableCollection<FileModel> _filesCollection = new();
        public ObservableCollection<FileModel> FilesCollection
        {
            get => _filesCollection;
            set => SetProperty(ref _filesCollection, value);
        }

        private SubtitleModel primaryFileSubtitlesSelectedItem;
        public SubtitleModel PrimaryFileSubtitlesSelectedItem
        {
            get => primaryFileSubtitlesSelectedItem;
            set
            {
                if (SetProperty(ref primaryFileSubtitlesSelectedItem, value) && value != null)
                {
                    SyncSecondarySelections(value.Position);
                }
            }
        }

        private FileModel secondaryFilesSelectedItem;
        public FileModel SecondaryFilesSelectedItem
        {
            get { return secondaryFilesSelectedItem; }
            set { SetProperty(ref secondaryFilesSelectedItem, value); }
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

        private DelegateCommand<FileOperationEnum?> _fileOperation;
        public DelegateCommand<FileOperationEnum?> FileOperationCommand =>
            _fileOperation ?? (_fileOperation = new DelegateCommand<FileOperationEnum?>(ExecuteFileOperationCommand));

        void ExecuteFileOperationCommand(FileOperationEnum? parameter)
        {
            if (parameter.HasValue)
            {
                MessageBox.Show($"You triggered the {parameter.Value} operation. Implement the logic in ExecuteFileOperationCommand.");
                DoFileOperation(parameter.Value);
            }
        }

        private DelegateCommand<SubtitleModel?> promoteSubtitle;
        public DelegateCommand<SubtitleModel?> PromoteSubtitleCommand =>
            promoteSubtitle ?? (promoteSubtitle = new DelegateCommand<SubtitleModel?>(ExecutePromoteSubtitleCommand));

        private void ExecutePromoteSubtitleCommand(SubtitleModel parameter)
        {
            if (parameter == null) return;

            // A = 1 (Max), E = 5 (Min). 
            // To 'Promote' (make harder), we move towards A (decrease numeric value).
            // If it's already A (1), or it's None, we might not want to promote.
            // Assuming you want: None -> E -> D -> C -> B -> A

            SubtitleRatingEnum? newRating;

            if (parameter.RatingCurrent == null || parameter.RatingCurrent == SubtitleRatingEnum.None)
            {
                newRating = SubtitleRatingEnum.E;
            }
            else if (parameter.RatingCurrent == SubtitleRatingEnum.A)
            {
                return; // Already at max
            }
            else
            {
                newRating = (SubtitleRatingEnum)((int)parameter.RatingCurrent - 1);
            }

            ApplyRating(parameter, newRating);
        }

        private DelegateCommand<SubtitleModel?> demoteSubtitle;
        public DelegateCommand<SubtitleModel?> DemoteSubtitleCommand =>
            demoteSubtitle ?? (demoteSubtitle = new DelegateCommand<SubtitleModel?>(ExecuteDemoteSubtitleCommand));

        private void ExecuteDemoteSubtitleCommand(SubtitleModel parameter)
        {
            if (parameter == null || parameter.RatingCurrent == SubtitleRatingEnum.E) return;
            if (parameter.RatingCurrent == null || parameter.RatingCurrent == SubtitleRatingEnum.None) return;

            // Move towards E (increase numeric value)
            var newRating = (SubtitleRatingEnum)((int)parameter.RatingCurrent + 1);

            ApplyRating(parameter, newRating);
        }

        private DelegateCommand<SubtitleModel> removeRating;
        public DelegateCommand<SubtitleModel> RemoveRatingCommand =>
            removeRating ?? (removeRating = new DelegateCommand<SubtitleModel>(ExecuteRemoveRatingCommand));

        private void ExecuteRemoveRatingCommand(SubtitleModel parameter)
        {
            if (parameter == null) return;
            ApplyRating(parameter, null);
        }

        #endregion
    }
}
