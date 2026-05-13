// Subs.Ext\Tools\Rating.WPF\ViewModels\WorkspaceViewModel.cs

using LibVLCSharp.Shared;
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rating.WPF.ViewModels
{
    public class WorkspaceViewModel : BindableBase, IDisposable
    {
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IRatingService _ratingService;
        private readonly IFileOperationService _fileOps;
        private readonly ISubtitleSyncService _syncService;
        private readonly IMediaService _mediaService;
        private readonly ISettingsService _settingsService;

        public WorkspaceViewModel(
            IFileService fileService,
            IDialogService dialogService,
            IRatingService ratingService,
            IFileOperationService fileOps,
            ISubtitleSyncService syncService,
            IMediaService mediaService,
            ISettingsService settingsService)
        {
            _fileService = fileService;
            _dialogService = dialogService;
            _ratingService = ratingService;
            _fileOps = fileOps;
            _syncService = syncService;
            _mediaService = mediaService;
            _settingsService = settingsService;

            _mediaService.AudioTracksUpdated += tracks =>
            {
                MediaFileAudioTracks = tracks;
            };
        }

        private void RunMediaPlayer(FileRankEnum fileRank)
        {
            // --- VALIDATION ---------------------------------------------------------

            string validationError =
                string.IsNullOrEmpty(MediaFilename) ? "Please select a media file." :
                LanguageLevelSelectedItem == null ? "Please select your language level." :
                (fileRank == FileRankEnum.Primary && PrimaryFile == null) ? "Please select a primary subtitles file." :
                (fileRank == FileRankEnum.Secondary && SecondaryFilesSelectedItem == null) ? "Please select a secondary subtitles file." :
                null;

            if (validationError != null)
            {
                _dialogService.ShowDialog(nameof(NotificationDialog),
                    new DialogParameters { { "message", validationError } }, null);
                return;
            }

            // --- SELECT FILE MODEL --------------------------------------------------

            FileModel fileModel = fileRank == FileRankEnum.Primary
                ? PrimaryFile
                : SecondaryFilesSelectedItem;

            // --- COUNT RELEVANT SUBTITLES ------------------------------------------

            int relevantSubCount = fileModel.SubtitleCollection.Count(sub =>
                sub.RatingCurrent.HasValue &&
                (int)sub.RatingCurrent.Value < (int)LanguageLevelSelectedItem.Value);

            // --- PREPARE DIALOG PARAMETERS -----------------------------------------

            var parameters = new DialogParameters
    {
        { "fileRank", fileRank },
        { "mediaPath", MediaFilename },
        { "subtitlePath", fileModel.FilePath },
        { "myLanguageLevel", LanguageLevelSelectedItem.ToString() },
        { "relevantSubCount", relevantSubCount }
    };

            // --- SHOW DIALOG --------------------------------------------------------

            _dialogService.ShowDialog(nameof(RunMediaPlayerDialog), parameters, async dialogResult =>
            {
                if (dialogResult.Result != ButtonResult.OK)
                    return;

                if (!dialogResult.Parameters.ContainsKey("tempFileName"))
                    return;

                string tempFileName = dialogResult.Parameters.GetValue<string>("tempFileName");

                // Determine which file model to filter
                FileModel fileModel = fileRank == FileRankEnum.Primary
                    ? PrimaryFile
                    : SecondaryFilesSelectedItem;

                try
                {
                    // 1. Write filtered subtitles into the temp file
                    int writtenCount = await _fileService.WriteTmpFileAsync(
                        fileModel,
                        tempFileName,
                        LanguageLevelSelectedItem.Value);

                    if (writtenCount == 0)
                    {
                        _dialogService.ShowDialog(nameof(NotificationDialog),
                            new DialogParameters { { "message", "No subtitles match your selected difficulty level." } },
                            null);
                        return;
                    }

                    // 2. Launch standalone VLC with media + filtered subs
                    Task.Run(() =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = _settingsService.Settings.VlcPath,   // path to VLC.exe
                                Arguments = $"\"{MediaFilename}\" --sub-file=\"{tempFileName}\"",
                                UseShellExecute = false
                            });
                        }
                        catch (Exception ex)
                        {
                            _dialogService.ShowDialog(nameof(NotificationDialog),
                                new DialogParameters { { "message", $"Failed to launch VLC: {ex.Message}" } },
                                null);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _dialogService.ShowDialog(nameof(NotificationDialog),
                        new DialogParameters { { "message", $"Error creating temporary subtitle file: {ex.Message}" } },
                        null);
                }
            });
        }

        // ---------------------------------------------------------
        // FILE OPENING
        // ---------------------------------------------------------

        private async Task OpenSubtitlesFile(FileRankEnum fileRank)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SRT files (*.srt)|*.srt"
            };

            if (dialog.ShowDialog() != true) return;

            var fileModel = await _fileService.ReadFileAsync(dialog.FileName);
            fileModel.FileRank = fileRank;

            if (fileRank == FileRankEnum.Primary)
            {
                var existing = FilesCollection.FirstOrDefault(f => f.FileRank == FileRankEnum.Primary);
                if (existing != null) FilesCollection.Remove(existing);
            }

            FilesCollection.Add(fileModel);

            RaisePropertyChanged(nameof(PrimaryFile));
            RaisePropertyChanged(nameof(SecondaryFiles));

            if (fileRank == FileRankEnum.Primary)
            {
                PrimaryFileSubtitlesSelectedItem = fileModel.SubtitleCollection.FirstOrDefault();
            }
            else if (PrimaryFileSubtitlesSelectedItem != null)
            {
                _syncService.SyncSecondarySelections(SecondaryFiles, PrimaryFileSubtitlesSelectedItem.Position);
            }
        }

        // ---------------------------------------------------------
        // PROPERTIES
        // ---------------------------------------------------------

        public FileModel PrimaryFile => FilesCollection.FirstOrDefault(f => f.FileRank == FileRankEnum.Primary);
        public IEnumerable<FileModel> SecondaryFiles => FilesCollection.Where(f => f.FileRank == FileRankEnum.Secondary);

        private ObservableCollection<FileModel> _filesCollection = new();
        public ObservableCollection<FileModel> FilesCollection
        {
            get => _filesCollection;
            set => SetProperty(ref _filesCollection, value);
        }

        private SubtitleModel _primarySelected;
        public SubtitleModel PrimaryFileSubtitlesSelectedItem
        {
            get => _primarySelected;
            set
            {
                if (SetProperty(ref _primarySelected, value) && value != null)
                {
                    _syncService.SyncSecondarySelections(SecondaryFiles, value.Position);
                    _mediaService.SeekTo(value.TimeFrom);
                }
            }
        }

        private FileModel _secondarySelected;
        public FileModel SecondaryFilesSelectedItem
        {
            get => _secondarySelected;
            set => SetProperty(ref _secondarySelected, value);
        }

        public Array LanguageLevelArray
        {
            get { return Enum.GetValues(typeof(MyLanguageLevelEnum)); }
        }

        private MyLanguageLevelEnum? _languageLevelSelectedItem;
        public MyLanguageLevelEnum? LanguageLevelSelectedItem
        {
            get { return _languageLevelSelectedItem; }
            set { SetProperty(ref _languageLevelSelectedItem, value); }
        }

        // MEDIA PROPERTIES (bound to MediaService)
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            set => SetProperty(ref _mediaPlayer, value);
        }
        private MediaPlayer _mediaPlayer;

        public string MediaFilename
        {
            get => _mediaFilename;
            set => SetProperty(ref _mediaFilename, value);
        }
        private string _mediaFilename;

        public bool MediaPlayerIsMuted
        {
            get => _mediaMuted;
            set
            {
                SetProperty(ref _mediaMuted, value);
                _mediaService.SetMute(value);
            }
        }
        private bool _mediaMuted;

        public bool MediaPlayerIsPaused
        {
            get => _mediaPaused;
            set
            {
                SetProperty(ref _mediaPaused, value);
                _mediaService.SetPause(value);
            }
        }
        private bool _mediaPaused;

        public List<LibVLCSharp.Shared.Structures.TrackDescription> MediaFileAudioTracks
        {
            get => _audioTracks;
            set => SetProperty(ref _audioTracks, value);
        }
        private List<LibVLCSharp.Shared.Structures.TrackDescription> _audioTracks;

        public LibVLCSharp.Shared.Structures.TrackDescription? MediaFileAudioTrackSelectedItem
        {
            get => _audioTrackSelected;
            set
            {
                SetProperty(ref _audioTrackSelected, value);
                _mediaService.SelectedAudioTrack = value;
            }
        }
        private LibVLCSharp.Shared.Structures.TrackDescription? _audioTrackSelected;

        // ---------------------------------------------------------
        // COMMANDS
        // ---------------------------------------------------------

        public DelegateCommand<FileRankEnum?> RunMediaPlayerCommand =>
            _runMediaPlayer ??= new DelegateCommand<FileRankEnum?>(async p =>
            {
                if (!p.HasValue) return;
                RunMediaPlayer(p.Value);
            });
        private DelegateCommand<FileRankEnum?> _runMediaPlayer;

        public DelegateCommand<FileRankEnum?> OpenSubtitlesFileCommand =>
            _openSubs ??= new DelegateCommand<FileRankEnum?>(async p =>
            {
                if (p.HasValue)
                    await OpenSubtitlesFile(p.Value);
            });
        private DelegateCommand<FileRankEnum?> _openSubs;

        public DelegateCommand OpenSettingsCommand =>
            _openSettings ??= new DelegateCommand(() =>
            {
                _dialogService.ShowDialog(nameof(SettingsDialog), null, null);
            });
        private DelegateCommand _openSettings;

        public DelegateCommand OpenHelpCommand =>
            _openHelp ??= new DelegateCommand(() =>
            {
                _dialogService.ShowDialog(nameof(HelpDialog), null, null);
            });
        private DelegateCommand _openHelp;

        public DelegateCommand OpenMediaFileCommand =>
            _openMedia ??= new DelegateCommand(() =>
            {
                _mediaService.OpenMediaFile();
                MediaPlayer = _mediaService.MediaPlayer;
                MediaFilename = _mediaService.MediaFilename;
                //MediaFileAudioTracks = _mediaService.AudioTracks;
            });
        private DelegateCommand _openMedia;

        public DelegateCommand ReplaySubtitleCommand =>
            _replay ??= new DelegateCommand(() =>
            {
                if (PrimaryFileSubtitlesSelectedItem != null)
                    _mediaService.SeekTo(PrimaryFileSubtitlesSelectedItem.TimeFrom);
            });
        private DelegateCommand _replay;

        public DelegateCommand<FileOperationEnum?> FileOperationCommand =>
            _fileOpsCmd ??= new DelegateCommand<FileOperationEnum?>(async op =>
            {
                if (op.HasValue)
                {
                    await _fileOps.ExecuteAsync(
                        op.Value,
                        FilesCollection,
                        PrimaryFile,
                        SecondaryFilesSelectedItem,
                        _fileService,
                        _dialogService).ContinueWith(t =>
                        {
                            RaisePropertyChanged(nameof(SecondaryFilesSelectedItem));
                            RaisePropertyChanged(nameof(SecondaryFiles));
                        });
                }
            });
        private DelegateCommand<FileOperationEnum?> _fileOpsCmd;

        public DelegateCommand<SubtitleModel> PromoteSubtitleCommand =>
            _promote ??= new DelegateCommand<SubtitleModel>(sub =>
            {
                _ratingService.Promote(FilesCollection, sub);
            });
        private DelegateCommand<SubtitleModel> _promote;

        public DelegateCommand<SubtitleModel> DemoteSubtitleCommand =>
            _demote ??= new DelegateCommand<SubtitleModel>(sub =>
            {
                _ratingService.Demote(FilesCollection, sub);
            });
        private DelegateCommand<SubtitleModel> _demote;

        public DelegateCommand<SubtitleModel> RemoveRatingCommand =>
            _remove ??= new DelegateCommand<SubtitleModel>(sub =>
            {
                _ratingService.Remove(FilesCollection, sub);
            });
        private DelegateCommand<SubtitleModel> _remove;

        // ---------------------------------------------------------
        // DISPOSAL
        // ---------------------------------------------------------

        public void Dispose()
        {
            _mediaService.Dispose();
        }
    }
}