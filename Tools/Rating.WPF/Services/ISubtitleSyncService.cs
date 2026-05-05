using Rating.WPF.Models;
using System.Collections.Generic;

namespace Rating.WPF.Services
{
    public interface ISubtitleSyncService
    {
        void SyncSecondarySelections(IEnumerable<FileModel> secondaryFiles, int position);
    }
}