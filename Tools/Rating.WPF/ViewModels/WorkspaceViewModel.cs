// Subs.Ext\Tools\Rating.WPF\ViewModels\WorkspaceViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        private readonly IFileService _fileService;
        private readonly int _subtitleRatingEnumMaxVal = Enum.GetValues(typeof(SubtitleRatingEnum)).Cast<int>().Max();
        private readonly int _subtitleRatingEnumMinVal = Enum.GetValues(typeof(SubtitleRatingEnum)).Cast<int>().Min();

        #endregion

        #region CTOR

        public WorkspaceViewModel(IFileService fileService)
        {
            this._fileService = fileService;
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
                fileModel = await _fileService.ReadFileAsync(filePath, cancellationToken);
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
                    this.PrimaryFileSubtitleSelectedItem = this.PrimaryFile.SubtitleCollection.FirstOrDefault();
                    break;

                case FileRankEnum.Secondary:

                    this.SecondaryFileCollection.Add(fileModel);

                    if(this.PrimaryFileSubtitleSelectedItem != null)
                    {
                        SetSecondaryFilesSubtitleSelectedItem(this.PrimaryFileSubtitleSelectedItem.Position);
                    }

                    break;
            }
        }

        private void ApplyRating(SubtitleModel target, SubtitleRatingEnum newRating)
        {
            // Check if this belongs to the Primary file
            if (PrimaryFile != null && PrimaryFile.SubtitleCollection.Contains(target))
            {
                // Sync all secondary files by Position (Index)
                foreach (var file in SecondaryFileCollection)
                {
                    var matchingSub = file.SubtitleCollection
                        .FirstOrDefault(s => s.Position == target.Position);

                    if (matchingSub != null)
                    {
                        matchingSub.RatingCurrent = newRating;
                    }
                }
            }
        }

        private void SetSecondaryFilesSubtitleSelectedItem(int position)
        {
            foreach (var file in SecondaryFileCollection)
            {
                file.SubtitleSelectedItem = file.SubtitleCollection.FirstOrDefault(s => s.Position == position);
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

        private SubtitleModel primaryFileSubtitleSelectedItem;
        public SubtitleModel PrimaryFileSubtitleSelectedItem
        {
            get { return primaryFileSubtitleSelectedItem; }
            set
            {
                SetProperty(ref primaryFileSubtitleSelectedItem, value);

                if (value != null)
                {
                    SetSecondaryFilesSubtitleSelectedItem(value.Position);
                }
            }
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

        private DelegateCommand<SubtitleModel> promoteSubtitle;
        public DelegateCommand<SubtitleModel> PromoteSubtitleCommand =>
            promoteSubtitle ?? (promoteSubtitle = new DelegateCommand<SubtitleModel>(ExecutePromoteSubtitleCommand));

        private void ExecutePromoteSubtitleCommand(SubtitleModel parameter)
        {
            if (parameter == null || (int)parameter.RatingCurrent == _subtitleRatingEnumMaxVal) return;

            // Increment the rating of the current subtitle
            parameter.RatingCurrent = parameter.RatingCurrent++;

            // Check if the subtitle belongs to the Primary file and sync all secondary files respecting the Position (Index)
            ApplyRating(parameter, parameter.RatingCurrent.Value);
        }

        private DelegateCommand<SubtitleModel> demoteSubtitle;
        public DelegateCommand<SubtitleModel> DemoteSubtitleCommand =>
            demoteSubtitle ?? (demoteSubtitle = new DelegateCommand<SubtitleModel>(ExecuteDemoteSubtitleCommand));

        private void ExecuteDemoteSubtitleCommand(SubtitleModel parameter)
        {
            if (parameter == null || (int)parameter.RatingCurrent == _subtitleRatingEnumMinVal) return;

            // Decrement the rating of the current subtitle
            parameter.RatingCurrent = parameter.RatingCurrent--;

            // Check if the subtitle belongs to the Primary file and sync all secondary files respecting the Position (Index)
            ApplyRating(parameter, parameter.RatingCurrent.Value);
        }

        #endregion
    }
}
