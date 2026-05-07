// Subs.Ext\Tools\Rating.WPF\Services\SubtitleSyncService.cs

using Rating.WPF.Models;
using System.Collections.Generic;
using System.Linq;

namespace Rating.WPF.Services
{
    public class SubtitleSyncService : ISubtitleSyncService
    {
        public void SyncSecondarySelections(IEnumerable<FileModel> secondaryFiles, int position)
        {
            if (secondaryFiles == null)
                return;

            foreach (var file in secondaryFiles)
            {
                var match = file.SubtitleCollection
                    .FirstOrDefault(s => s.Position == position);

                file.SubtitleSelectedItem = match;
            }
        }
    }
}