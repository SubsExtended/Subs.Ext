using Rating.WPF.Enums;
using Rating.WPF.Models;
using System.Collections.ObjectModel;

namespace Rating.WPF.Services
{
    public interface IRatingService
    {
        void ApplyRating(ObservableCollection<FileModel> files, SubtitleModel subtitle, SubtitleRatingEnum? newRating);
        void Promote(ObservableCollection<FileModel> files, SubtitleModel subtitle);
        void Demote(ObservableCollection<FileModel> files, SubtitleModel subtitle);
        void Remove(ObservableCollection<FileModel> files, SubtitleModel subtitle);
    }
}