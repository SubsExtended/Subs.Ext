using Xunit;
using System.Collections.ObjectModel;
using Rating.WPF.Enums;
using Rating.WPF.Models;
using Rating.WPF.Services;

namespace Rating.WPF.Tests;

public sealed class RatingServiceTests
{
    private readonly RatingService _sut = new();

    [Fact]
    public void ApplyRating_UpdatesSelectedSubtitleAndMarksOwningFileDirty()
    {
        var file = CreateFile(FileRankEnum.Primary, CreateSubtitle(1, (SubtitleRatingEnum)2, (SubtitleRatingEnum)2));
        var subtitle = file.SubtitleCollection[0];

        _sut.ApplyRating(new ObservableCollection<FileModel> { file }, subtitle, (SubtitleRatingEnum)3);

        Assert.Equal((SubtitleRatingEnum)3, subtitle.RatingCurrent);
        Assert.True(file.IsDirty);
    }

    [Fact]
    public void ApplyRating_WhenPrimary_PropagatesRatingToSecondaryWithSamePosition()
    {
        var primarySubtitle = CreateSubtitle(7, (SubtitleRatingEnum)1, (SubtitleRatingEnum)1);
        var secondarySubtitle = CreateSubtitle(7, null, null);
        var primary = CreateFile(FileRankEnum.Primary, primarySubtitle);
        var secondary = CreateFile(FileRankEnum.Secondary, secondarySubtitle);

        _sut.ApplyRating(new ObservableCollection<FileModel> { primary, secondary }, primarySubtitle, (SubtitleRatingEnum)3);

        Assert.Equal((SubtitleRatingEnum)3, primarySubtitle.RatingCurrent);
        Assert.Equal((SubtitleRatingEnum)3, secondarySubtitle.RatingCurrent);
        Assert.True(primary.IsDirty);
        Assert.True(secondary.IsDirty);
    }

    [Fact]
    public void ApplyRating_WhenSecondary_DoesNotPropagateToOtherFiles()
    {
        var primarySubtitle = CreateSubtitle(7, (SubtitleRatingEnum)1, (SubtitleRatingEnum)1);
        var secondarySubtitle = CreateSubtitle(7, null, null);
        var primary = CreateFile(FileRankEnum.Primary, primarySubtitle);
        var secondary = CreateFile(FileRankEnum.Secondary, secondarySubtitle);

        _sut.ApplyRating(new ObservableCollection<FileModel> { primary, secondary }, secondarySubtitle, (SubtitleRatingEnum)2);

        Assert.Equal((SubtitleRatingEnum)1, primarySubtitle.RatingCurrent);
        Assert.Equal((SubtitleRatingEnum)2, secondarySubtitle.RatingCurrent);
        Assert.False(primary.IsDirty);
        Assert.True(secondary.IsDirty);
    }

    [Fact]
    public void ApplyRating_WithUnknownSubtitle_DoesNothing()
    {
        var file = CreateFile(FileRankEnum.Primary, CreateSubtitle(1, null, null));
        var unknown = CreateSubtitle(2, null, null);

        _sut.ApplyRating(new ObservableCollection<FileModel> { file }, unknown, (SubtitleRatingEnum)2);

        Assert.Null(unknown.RatingCurrent);
        Assert.False(file.IsDirty);
    }

    [Fact]
    public void ApplyRating_WithNullSubtitle_DoesNothing()
    {
        var file = CreateFile(FileRankEnum.Primary, CreateSubtitle(1, null, null));
        _sut.ApplyRating(new ObservableCollection<FileModel> { file }, null, (SubtitleRatingEnum)2);
        Assert.False(file.IsDirty);
    }

    [Fact]
    public void Promote_FromUnrated_SetsRatingToE()
    {
        var subtitle = CreateSubtitle(1, null, null);
        var file = CreateFile(FileRankEnum.Primary, subtitle);

        _sut.Promote(new ObservableCollection<FileModel> { file }, subtitle);

        Assert.Equal((SubtitleRatingEnum)4, subtitle.RatingCurrent);
    }

    [Fact]
    public void Promote_DecreasesEnumValueByOne()
    {
        var subtitle = CreateSubtitle(1, (SubtitleRatingEnum)3, (SubtitleRatingEnum)3);
        var file = CreateFile(FileRankEnum.Primary, subtitle);

        _sut.Promote(new ObservableCollection<FileModel> { file }, subtitle);

        Assert.Equal((SubtitleRatingEnum)2, subtitle.RatingCurrent);
    }

    [Fact]
    public void Promote_FromA_DoesNotChangeRating()
    {
        var subtitle = CreateSubtitle(1, (SubtitleRatingEnum)0, (SubtitleRatingEnum)0);
        var file = CreateFile(FileRankEnum.Primary, subtitle);

        _sut.Promote(new ObservableCollection<FileModel> { file }, subtitle);

        Assert.Equal((SubtitleRatingEnum)0, subtitle.RatingCurrent);
        Assert.False(file.IsDirty);
    }

    [Fact]
    public void Demote_FromD_SetsE()
    {
        var subtitle = CreateSubtitle(1, (SubtitleRatingEnum)3, (SubtitleRatingEnum)3);
        var file = CreateFile(FileRankEnum.Primary, subtitle);

        _sut.Demote(new ObservableCollection<FileModel> { file }, subtitle);

        Assert.Equal((SubtitleRatingEnum)4, subtitle.RatingCurrent);
    }

    [Fact]
    public void Demote_FromE_DoesNotChangeRating()
    {
        var subtitle = CreateSubtitle(1, (SubtitleRatingEnum)4, (SubtitleRatingEnum)4);
        var file = CreateFile(FileRankEnum.Primary, subtitle);

        _sut.Demote(new ObservableCollection<FileModel> { file }, subtitle);

        Assert.Equal((SubtitleRatingEnum)4, subtitle.RatingCurrent);
        Assert.False(file.IsDirty);
    }

    [Fact]
    public void Remove_ClearsCurrentRatingAndMarksFileDirty()
    {
        var subtitle = CreateSubtitle(1, (SubtitleRatingEnum)2, (SubtitleRatingEnum)2);
        var file = CreateFile(FileRankEnum.Primary, subtitle);

        _sut.Remove(new ObservableCollection<FileModel> { file }, subtitle);

        Assert.Null(subtitle.RatingCurrent);
        Assert.True(file.IsDirty);
    }

    private static FileModel CreateFile(FileRankEnum rank, params SubtitleModel[] subtitles) => new()
    {
        FileRank = rank,
        SubtitleCollection = new ObservableCollection<SubtitleModel>(subtitles)
    };

    private static SubtitleModel CreateSubtitle(int position, SubtitleRatingEnum? original, SubtitleRatingEnum? current) => new(Guid.NewGuid())
    {
        Position = position,
        RatingOriginal = original,
        RatingCurrent = current
    };
}
