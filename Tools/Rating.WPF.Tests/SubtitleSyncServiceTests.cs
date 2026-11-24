using Xunit;
using System.Collections.ObjectModel;
using Rating.WPF.Models;
using Rating.WPF.Services;

namespace Rating.WPF.Tests;

public sealed class SubtitleSyncServiceTests
{
    private readonly SubtitleSyncService _sut = new();

    [Fact]
    public void SyncSecondarySelections_SelectsMatchingPositionInEachSecondary()
    {
        var firstMatch = CreateSubtitle(12);
        var secondMatch = CreateSubtitle(12);
        var files = new[]
        {
            CreateFile(CreateSubtitle(11), firstMatch),
            CreateFile(secondMatch, CreateSubtitle(13))
        };

        _sut.SyncSecondarySelections(files, 12);

        Assert.Same(firstMatch, files[0].SubtitleSelectedItem);
        Assert.Same(secondMatch, files[1].SubtitleSelectedItem);
    }

    [Fact]
    public void SyncSecondarySelections_WhenPositionMissing_ClearsSelection()
    {
        var file = CreateFile(CreateSubtitle(1));
        file.SubtitleSelectedItem = file.SubtitleCollection[0];

        _sut.SyncSecondarySelections(new[] { file }, 99);

        Assert.Null(file.SubtitleSelectedItem);
    }

    [Fact]
    public void SyncSecondarySelections_WithNullCollection_DoesNothing()
    {
        _sut.SyncSecondarySelections(null, 1);
    }

    private static FileModel CreateFile(params SubtitleModel[] subtitles) => new()
    {
        SubtitleCollection = new ObservableCollection<SubtitleModel>(subtitles)
    };

    private static SubtitleModel CreateSubtitle(int position) => new(Guid.NewGuid()) { Position = position };
}
