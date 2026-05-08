// Subs.Ext\Tools\Rating.WPF\Services\RatingService.cs

using Rating.WPF.Enums;
using Rating.WPF.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rating.WPF.Services
{
    public class RatingService : IRatingService
    {
        public void ApplyRating(ObservableCollection<FileModel> files, SubtitleModel subtitle, SubtitleRatingEnum? newRating)
        {
            if (subtitle == null) return;

            // Find the file that owns this subtitle
            var fileWithSubtitle = files.FirstOrDefault(f => f.SubtitleCollection.Any(s => s.PK == subtitle.PK));

            if (fileWithSubtitle == null) return;

            // Always update the clicked subtitle
            subtitle.RatingCurrent = newRating;

            // If primary → sync to all secondaries
            if (fileWithSubtitle.FileRank == FileRankEnum.Primary)
            {
                foreach (var file in files)
                {
                    if (file == fileWithSubtitle) continue;

                    var sub = file.SubtitleCollection.FirstOrDefault(s => s.Position == subtitle.Position);

                    if (sub != null) sub.RatingCurrent = newRating;
                }
            }

            // Update dirty flags
            foreach (var file in files)
            {
                file.SetIsDirty();
            }
        }

        public void Promote(ObservableCollection<FileModel> files, SubtitleModel subtitle)
        {
            if (subtitle == null) return;

            SubtitleRatingEnum? newRating;

            if (subtitle.RatingCurrent == null || subtitle.RatingCurrent == SubtitleRatingEnum.None)
            {
                newRating = SubtitleRatingEnum.E;
            }
            else if (subtitle.RatingCurrent == SubtitleRatingEnum.A)
            {
                return; // Already max
            }
            else
            {
                newRating = (SubtitleRatingEnum)((int)subtitle.RatingCurrent - 1);
            }

            ApplyRating(files, subtitle, newRating);
        }

        public void Demote(ObservableCollection<FileModel> files, SubtitleModel subtitle)
        {
            if (subtitle == null) return;
            if (subtitle.RatingCurrent == null || subtitle.RatingCurrent == SubtitleRatingEnum.None) return;
            if (subtitle.RatingCurrent == SubtitleRatingEnum.E) return;

            var newRating = (SubtitleRatingEnum)((int)subtitle.RatingCurrent + 1);

            ApplyRating(files, subtitle, newRating);
        }

        public void Remove(ObservableCollection<FileModel> files, SubtitleModel subtitle)
        {
            if (subtitle == null) return;

            ApplyRating(files, subtitle, null);
        }
    }
}