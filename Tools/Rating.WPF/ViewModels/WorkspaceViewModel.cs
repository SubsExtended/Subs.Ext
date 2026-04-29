// Subs.Ext\Tools\Rating.WPF\ViewModels\WorkspaceViewModel.cs

using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Rating.WPF.Dialogs;
using Rating.WPF.Enums;
using Rating.WPF.Models;
using Rating.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rating.WPF.ViewModels
{
    public class WorkspaceViewModel : BindableBase, IDisposable
    {
        #region Fields

        private readonly IFileService _fileService;
        private IDialogService _dialogService;
        private LibVLC _libVLC;

        #endregion

        #region CTOR

        public WorkspaceViewModel(IFileService fileService, IDialogService dialogService)
        {
            this._fileService = fileService;
            this._dialogService = dialogService;

            // Initialize VLC
            Core.Initialize();
            this._libVLC = new LibVLC();
            MediaPlayer = new MediaPlayer(_libVLC);
        }

        #endregion

        #region Methods

        private async Task OpenSubtitlesFile(FileRankEnum fileRank)
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
                if (existing != null) return;
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

        private bool ShowYesNoDialog(string message)
        {
            bool returnValue = false;

            _dialogService.ShowDialog(nameof(YesNoDialog), new DialogParameters($"message={message}"), r =>
            {
                if (r.Result == ButtonResult.Yes)
                {
                    returnValue = true;
                }
                else
                {
                    returnValue = false;
                }
            });

            return returnValue;
        }

        private async Task DoFileOperation(FileOperationEnum operation)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SRT files (*.srt)|*.srt",
            };

            switch (operation)
            {
                case FileOperationEnum.PrimarySave:
                    if (PrimaryFile == null) return;
                    if (!PrimaryFile.IsDirty) return;

                    await _fileService.WriteFileAsync(PrimaryFile, PrimaryFile.FilePath, new CancellationToken());
                    FinalizeFileSave(PrimaryFile);

                    break;

                case FileOperationEnum.PrimarySaveAs:
                    if (PrimaryFile == null) return;

                    saveFileDialog.FileName = Path.GetFileName(PrimaryFile.FilePath);
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        await _fileService.WriteFileAsync(PrimaryFile, saveFileDialog.FileName, new CancellationToken());
                        PrimaryFile.FilePath = saveFileDialog.FileName;
                        FinalizeFileSave(PrimaryFile);
                    }

                    break;

                case FileOperationEnum.PrimaryClose:
                    if (PrimaryFile == null) return;
                    if (PrimaryFile.IsDirty)
                    {
                        var result = ShowYesNoDialog($"You have unsaved changes in \r\n{PrimaryFile.FilePath}. Do you want to save before closing?");
                        if (result)
                        {
                            await _fileService.WriteFileAsync(PrimaryFile, PrimaryFile.FilePath, new CancellationToken());
                            FilesCollection.Remove(PrimaryFile);
                        }
                    }
                    else
                    {
                        FilesCollection.Remove(PrimaryFile);
                    }

                    break;

                case FileOperationEnum.SecondaryAllSave:
                    if (!SecondaryFiles.Any()) return;

                    foreach (var file in SecondaryFiles)
                    {
                        if (!file.IsDirty) continue;
                        await _fileService.WriteFileAsync(file, file.FilePath, new CancellationToken());
                        FinalizeFileSave(file);
                    }

                    break;

                case FileOperationEnum.SecondaryAllClose:
                    if (!SecondaryFiles.Any()) return;

                    List<Guid> filesToRemove = new List<Guid>();
                    foreach (var file in SecondaryFiles)
                    {
                        if (file.IsDirty)
                        {
                            var result = ShowYesNoDialog($"You have unsaved changes in \r\n{file.FilePath}. Do you want to save before closing?");
                            if (result)
                            {
                                await _fileService.WriteFileAsync(file, file.FilePath, new CancellationToken());
                                filesToRemove.Add(file.PK);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            filesToRemove.Add(file.PK);
                        }

                        foreach (var v in filesToRemove)
                        {
                            var fileToRemove = FilesCollection.FirstOrDefault(f => f.PK == v);
                            if (fileToRemove != null)
                            {
                                FilesCollection.Remove(fileToRemove);
                            }
                        }
                    }

                    break;

                case FileOperationEnum.SecondarySingleSave:
                    if (SecondaryFilesSelectedItem == null) return;
                    if (!SecondaryFilesSelectedItem.IsDirty) return;

                    await _fileService.WriteFileAsync(SecondaryFilesSelectedItem, SecondaryFilesSelectedItem.FilePath, new CancellationToken());
                    FinalizeFileSave(SecondaryFilesSelectedItem);

                    break;

                case FileOperationEnum.SecondarySingleSaveAs:
                    if (SecondaryFilesSelectedItem == null) return;

                    saveFileDialog.FileName = Path.GetFileName(SecondaryFilesSelectedItem.FilePath);

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        await _fileService.WriteFileAsync(SecondaryFilesSelectedItem, saveFileDialog.FileName, new CancellationToken());
                        SecondaryFilesSelectedItem.FilePath = saveFileDialog.FileName;
                        FinalizeFileSave(SecondaryFilesSelectedItem);
                    }

                    break;

                case FileOperationEnum.SecondarySingleClose:
                    if (SecondaryFilesSelectedItem == null) return;
                    if (SecondaryFilesSelectedItem.IsDirty)
                    {
                        var result = ShowYesNoDialog($"You have unsaved changes in \r\n{SecondaryFilesSelectedItem.FilePath}. Do you want to save before closing?");
                        if (result)
                        {
                            await _fileService.WriteFileAsync(SecondaryFilesSelectedItem, SecondaryFilesSelectedItem.FilePath, new CancellationToken());
                            FilesCollection.Remove(SecondaryFilesSelectedItem);
                        }
                    }
                    else
                    {
                        FilesCollection.Remove(SecondaryFilesSelectedItem);
                    }
                    break;

                default:
                    break;
            }
        }

        private void FinalizeFileSave(FileModel file)
        {
            file.IsDirty = false;

            foreach (var sub in file.SubtitleCollection)
            {
                sub.RatingOriginal = sub.RatingCurrent;
            }
        }

        private void OpenMediaFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video/audio Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.mp3;*.wav;*.flac"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (MediaPlayer != null)
                {
                    if (MediaPlayer.IsPlaying)
                    {
                        MediaPlayer.Stop();
                    }

                    MediaPlayer.Playing -= MediaPlayer_Playing;
                    MediaPlayer.Media?.Dispose();
                    MediaPlayer.Dispose();
                }

                using var media = new Media(_libVLC, new Uri(openFileDialog.FileName));

                MediaPlayer = new MediaPlayer(media);
                MediaPlayer.Playing += (sender, args) =>
                {
                    MediaFileAudioTracks = GetAudioTrackList();
                };
                MediaPlayer.SetSpu(-1);
                MediaPlayer.Play();
                MediaFilename = openFileDialog.FileName;
            }
        }

        private List<TrackDescription> GetAudioTrackList()
        {
            // Accessing AudioTrackDescription returns a collection of TrackDescription objects
            // Each object has an 'Id' (int) and a 'Name' (string)
            return MediaPlayer?.AudioTrackDescription.ToList() ?? new List<TrackDescription>();
        }

        // Command to seek to a specific subtitle's time
        private void SeekToPrimaryFileSubtitlesSelectedItem()
        {
            if (PrimaryFileSubtitlesSelectedItem != null && MediaPlayer != null && MediaPlayer.Media != null)
            {
                MediaPlayer.Time = (long)PrimaryFileSubtitlesSelectedItem.TimeFrom.TotalMilliseconds;
            }
        }

        private void MuteMediaPlayer(bool mute)
        {
            if (MediaPlayer != null && MediaPlayer.Media != null)
            {
                MediaPlayer.Mute = mute;
            }
        }

        private void PauseMediaPlayer(bool pause)
        {
            if (MediaPlayer != null && MediaPlayer.Media != null)
            {
                if (pause)
                {
                    MediaPlayer.Pause();
                }
                else
                {
                    MediaPlayer.Play();
                }
            }
        }

        #endregion

        #region Events

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            // Use Dispatcher.Invoke if you are updating UI from a background thread
            App.Current.Dispatcher.Invoke(() =>
            {
                MediaFileAudioTracks = GetAudioTrackList();

            });
        }

        #endregion

        #region Properties

        // UI Bindings for the two panels
        public FileModel PrimaryFile => FilesCollection.FirstOrDefault(f => f.FileRank == FileRankEnum.Primary);
        public IEnumerable<FileModel> SecondaryFiles => FilesCollection.Where(f => f.FileRank == FileRankEnum.Secondary);

        private ObservableCollection<FileModel> filesCollection = new();
        public ObservableCollection<FileModel> FilesCollection
        {
            get { return filesCollection; }
            set { SetProperty(ref filesCollection, value); }
        }

        private SubtitleModel primaryFileSubtitlesSelectedItem;
        public SubtitleModel PrimaryFileSubtitlesSelectedItem
        {
            get { return primaryFileSubtitlesSelectedItem; }
            set
            {
                if (SetProperty(ref primaryFileSubtitlesSelectedItem, value) && value != null)
                {
                    SyncSecondarySelections(value.Position);
                }

                if (value != null)
                {
                    SeekToPrimaryFileSubtitlesSelectedItem();
                }
            }
        }

        private FileModel secondaryFilesSelectedItem;
        public FileModel SecondaryFilesSelectedItem
        {
            get { return secondaryFilesSelectedItem; }
            set { SetProperty(ref secondaryFilesSelectedItem, value); }
        }

        private MediaPlayer mediaPlayer;
        public MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
            set { SetProperty(ref mediaPlayer, value); }
        }

        private bool mediaPlayerIsMuted;
        public bool MediaPlayerIsMuted
        {
            get { return mediaPlayerIsMuted; }
            set
            {
                SetProperty(ref mediaPlayerIsMuted, value);
                MuteMediaPlayer(value);
            }
        }

        private bool mediaPlayerIsPaused;
        public bool MediaPlayerIsPaused
        {
            get { return mediaPlayerIsPaused; }
            set
            {
                SetProperty(ref mediaPlayerIsPaused, value);
                PauseMediaPlayer(value);
            }
        }

        private string mediaFilename;
        public string MediaFilename
        {
            get { return mediaFilename; }
            set { SetProperty(ref mediaFilename, value); }
        }

        private List<TrackDescription> mediaFileAudioTracks;
        public List<TrackDescription> MediaFileAudioTracks
        {
            get => mediaFileAudioTracks;
            set => SetProperty(ref mediaFileAudioTracks, value);
        }

        private TrackDescription? mediaFileAudioTrackSelectedItem;
        public TrackDescription? MediaFileAudioTrackSelectedItem
        {
            get { return mediaFileAudioTrackSelectedItem; }
            set
            {
                SetProperty(ref mediaFileAudioTrackSelectedItem, value);

                if (value != null && MediaPlayer != null)
                {
                    MediaPlayer.SetAudioTrack(value.Value.Id);
                }
            }
        }

        #endregion

        #region Commands

        private DelegateCommand<FileRankEnum?> openSubtitlesFileCommand;
        public DelegateCommand<FileRankEnum?> OpenSubtitlesFileCommand =>
            openSubtitlesFileCommand ?? (openSubtitlesFileCommand = new DelegateCommand<FileRankEnum?>(ExecuteOpenSubtitlesFileCommand));

        async void ExecuteOpenSubtitlesFileCommand(FileRankEnum? parameter)
        {
            if (parameter.HasValue)
            {
                await OpenSubtitlesFile(parameter.Value);
            }
        }

        private DelegateCommand openMediaFileCommand;
        public DelegateCommand OpenMediaFileCommand =>
            openMediaFileCommand ?? (openMediaFileCommand = new DelegateCommand(ExecuteOpenMediaFileCommand));

        void ExecuteOpenMediaFileCommand()
        {
            OpenMediaFile();
        }

        private DelegateCommand replaySubtitleCommand;
        public DelegateCommand ReplaySubtitleCommand =>
            replaySubtitleCommand ?? (replaySubtitleCommand = new DelegateCommand(ExecuteReplaySubtitleCommand));

        void ExecuteReplaySubtitleCommand()
        {
            SeekToPrimaryFileSubtitlesSelectedItem();
        }

        private DelegateCommand<FileOperationEnum?> fileOperationCommand;
        public DelegateCommand<FileOperationEnum?> FileOperationCommand =>
            fileOperationCommand ?? (fileOperationCommand = new DelegateCommand<FileOperationEnum?>(ExecuteFileOperationCommand));

        async void ExecuteFileOperationCommand(FileOperationEnum? parameter)
        {
            if (parameter.HasValue)
            {
                await DoFileOperation(parameter.Value);
            }
        }

        private DelegateCommand<SubtitleModel> promoteSubtitleCommand;
        public DelegateCommand<SubtitleModel> PromoteSubtitleCommand =>
            promoteSubtitleCommand ?? (promoteSubtitleCommand = new DelegateCommand<SubtitleModel>(ExecutePromoteSubtitleCommand));

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

        private DelegateCommand<SubtitleModel> demoteSubtitleCommand;
        public DelegateCommand<SubtitleModel> DemoteSubtitleCommand =>
            demoteSubtitleCommand ?? (demoteSubtitleCommand = new DelegateCommand<SubtitleModel>(ExecuteDemoteSubtitleCommand));

        private void ExecuteDemoteSubtitleCommand(SubtitleModel parameter)
        {
            if (parameter == null || parameter.RatingCurrent == SubtitleRatingEnum.E) return;
            if (parameter.RatingCurrent == null || parameter.RatingCurrent == SubtitleRatingEnum.None) return;

            // Move towards E (increase numeric value)
            var newRating = (SubtitleRatingEnum)((int)parameter.RatingCurrent + 1);

            ApplyRating(parameter, newRating);
        }

        private DelegateCommand<SubtitleModel> removeRatingCommand;
        public DelegateCommand<SubtitleModel> RemoveRatingCommand =>
            removeRatingCommand ?? (removeRatingCommand = new DelegateCommand<SubtitleModel>(ExecuteRemoveRatingCommand));

        private void ExecuteRemoveRatingCommand(SubtitleModel parameter)
        {
            if (parameter == null) return;
            ApplyRating(parameter, null);
        }

        #endregion

        #region Implementation

        public void Dispose()
        {
            // Stop playback first
            MediaPlayer?.Stop();

            // Dispose in order: Player -> Media (if any) -> LibVLC
            MediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }

        #endregion
    }
}