using System;
using System.Collections.ObjectModel;

using Prism.Mvvm;

using Rating.WPF.Enums;
using Rating.WPF.Models;

namespace Rating.WPF.ViewModels
{
    public class WorkspaceViewViewModel : BindableBase
    {
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

        public WorkspaceViewViewModel()
        {
            PrimaryFile = new FileModel()
            {
                FileRank = FileRank.Primary,
                FileName = "PrimaryFile",
                FileType = FileType.Srt,
                Language = Language.English,
                Subs = new ObservableCollection<SubtitleModel>()
                {
                    new SubtitleModel() { SubtitleRating = SubRating.A, OriginalBlock = "PrimaryFile Sub1", Position = 1, Text = "PrimaryFile Sub1 Text", TimeFrom = new TimeSpan(0, 0, 0), TimeTo = new TimeSpan(0, 0, 5)},
                    new SubtitleModel() { SubtitleRating = SubRating.B, OriginalBlock = "PrimaryFile Sub2", Position = 2, Text = "PrimaryFile Sub2 Text", TimeFrom = new TimeSpan(0, 0, 5), TimeTo = new TimeSpan(0, 0, 10)},
                    new SubtitleModel() { SubtitleRating = SubRating.C, OriginalBlock = "PrimaryFile Sub3", Position = 3, Text = "PrimaryFile Sub3 Text", TimeFrom = new TimeSpan(0, 0, 10), TimeTo = new TimeSpan(0, 0, 15)},
                }
            };

            SecondaryFiles = new ObservableCollection<FileModel>()
            {
                new FileModel()
                {
                    FileRank = FileRank.Secondary,
                    FileName = "SecondaryFile1",
                    FileType = FileType.Srt,
                    Language = Language.French,
                    Subs = new ObservableCollection<SubtitleModel>()
                    {
                        new SubtitleModel() { SubtitleRating = SubRating.A, OriginalBlock = "SecondaryFile1 Sub1", Position = 1, Text = "SecondaryFile1 Sub1 Text", TimeFrom = new TimeSpan(0, 0, 0), TimeTo = new TimeSpan(0, 0, 5)},
                        new SubtitleModel() { SubtitleRating = SubRating.B, OriginalBlock = "SecondaryFile1 Sub2", Position = 2, Text = "SecondaryFile1 Sub2 Text", TimeFrom = new TimeSpan(0, 0, 5), TimeTo = new TimeSpan(0, 0, 10)},
                        new SubtitleModel() { SubtitleRating = SubRating.C, OriginalBlock = "SecondaryFile1 Sub3", Position = 3, Text = "SecondaryFile1 Sub3 Text", TimeFrom = new TimeSpan(0, 0, 10), TimeTo = new TimeSpan(0, 0, 15)},
                    }
                },
                new FileModel()
                {
                    FileRank = FileRank.Secondary,
                    FileName = "SecondaryFile2",
                    FileType = FileType.Srt,
                    Language = Language.Russian,
                    Subs = new ObservableCollection<SubtitleModel>()
                    {
                        new SubtitleModel() { SubtitleRating = SubRating.A, OriginalBlock = "SecondaryFile2 Sub1", Position = 1, Text = "SecondaryFile2 Sub1 Text", TimeFrom = new TimeSpan(0, 0, 0), TimeTo = new TimeSpan(0, 0, 5)},
                        new SubtitleModel() { SubtitleRating = SubRating.B, OriginalBlock = "SecondaryFile2 Sub2", Position = 2, Text = "SecondaryFile2 Sub2 Text", TimeFrom = new TimeSpan(0, 0, 5), TimeTo = new TimeSpan(0, 0, 10)},
                        new SubtitleModel() { SubtitleRating = SubRating.C, OriginalBlock = "SecondaryFile2 Sub3", Position = 3, Text = "SecondaryFile2 Sub3 Text", TimeFrom = new TimeSpan(0, 0, 10), TimeTo = new TimeSpan(0, 0, 15)},
                    }
                },
                new FileModel()
                {
                    FileRank = FileRank.Secondary,
                    FileName = "SecondaryFile3",
                    FileType = FileType.Srt,
                    Language = Language.Spanish,
                    Subs = new ObservableCollection<SubtitleModel>()
                    {
                        new SubtitleModel() { SubtitleRating = SubRating.A, OriginalBlock = "SecondaryFile3 Sub1", Position = 1, Text = "SecondaryFile3 Sub1 Text", TimeFrom = new TimeSpan(0, 0, 0), TimeTo = new TimeSpan(0, 0, 5)},
                        new SubtitleModel() { SubtitleRating = SubRating.B, OriginalBlock = "SecondaryFile3 Sub2", Position = 2, Text = "SecondaryFile3 Sub2 Text", TimeFrom = new TimeSpan(0, 0, 5), TimeTo = new TimeSpan(0, 0, 10)},
                        new SubtitleModel() { SubtitleRating = SubRating.C, OriginalBlock = "SecondaryFile3 Sub3", Position = 3, Text = "SecondaryFile3 Sub3 Text", TimeFrom = new TimeSpan(0, 0, 10), TimeTo = new TimeSpan(0, 0, 15)},
                    }
                },
            };
        }
    }
}
