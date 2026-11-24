using System.Text;
using Rating.WPF.Enums;
using Rating.WPF.Models;
using Rating.WPF.Services;
using Xunit;

namespace Rating.WPF.Tests;

public sealed class FileServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "RatedSubs.Tests", Guid.NewGuid().ToString("N"));
    private readonly FileService _sut = new();

    public FileServiceTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public async Task ReadFileAsync_ParsesMultipleEntriesAndDifficultyTags()
    {
        var path = WriteInput("sample.srt",
            "1\n" +
            "00:00:01,250 --> 00:00:02,500\n" +
            "Hello there! {DIFF:B}\n" +
            "How are you?\n\n" +
            "2\n" +
            "00:00:03,000 --> 00:00:04,000\n" +
            "Goodbye.\n");

        var result = await _sut.ReadFileAsync(path);

        Assert.Equal(2, result.SubtitleCollection.Count);
        var first = result.SubtitleCollection[0];
        Assert.Equal(1, first.Position);
        Assert.Equal(TimeSpan.FromMilliseconds(1250), first.TimeFrom);
        Assert.Equal(TimeSpan.FromMilliseconds(2500), first.TimeTo);
        Assert.Equal($"Hello there!{Environment.NewLine}How are you?", first.Text);
        Assert.Equal((SubtitleRatingEnum)1, first.RatingOriginal);
        Assert.Equal(first.RatingOriginal, first.RatingCurrent);
        Assert.Null(result.SubtitleCollection[1].RatingCurrent);
    }

    [Fact]
    public async Task ReadFileAsync_ParsesTagCaseInsensitively()
    {
        var path = WriteInput("lowercase.srt", "1\n00:00:00,000 --> 00:00:01,000\nA line {diff:d}\n");

        var result = await _sut.ReadFileAsync(path);

        Assert.Equal((SubtitleRatingEnum)3, result.SubtitleCollection.Single().RatingCurrent);
        Assert.Equal("A line", result.SubtitleCollection.Single().Text);
    }

    [Fact]
    public async Task ReadFileAsync_HandlesLastEntryWithoutTrailingBlankLine()
    {
        var path = WriteInput("last.srt", "1\n00:00:00,000 --> 00:00:01,000\nLast line");

        var result = await _sut.ReadFileAsync(path);

        Assert.Single(result.SubtitleCollection);
        Assert.Equal("Last line", result.SubtitleCollection[0].Text);
    }

    [Fact]
    public async Task ReadFileAsync_ThrowsForMissingFile()
    {
        var path = Path.Combine(_directory, "missing.srt");
        await Assert.ThrowsAsync<FileNotFoundException>(() => _sut.ReadFileAsync(path));
    }

    [Fact]
    public async Task ReadFileAsync_CurrentImplementationMapsInvalidTimestampToZero()
    {
        // Characterizes current behavior. Prefer changing the production parser to reject this input,
        // then replace this assertion with Assert.ThrowsAsync / a documented parse-result assertion.
        var path = WriteInput("bad-time.srt", "1\nnot-a-time --> 00:00:01,000\nText\n");

        var result = await _sut.ReadFileAsync(path);

        Assert.Equal(TimeSpan.Zero, result.SubtitleCollection.Single().TimeFrom);
    }

    [Fact]
    public async Task WriteFileAsync_WritesCurrentRatingAndCanBeReadBack()
    {
        var model = CreateFileModel(CreateSubtitle(1, "A line", (SubtitleRatingEnum)2));
        var path = Path.Combine(_directory, "written.srt");

        await _sut.WriteFileAsync(model, path);
        var text = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var readBack = await _sut.ReadFileAsync(path);

        Assert.Contains("A line {DIFF:C}", text);
        Assert.Equal("A line", readBack.SubtitleCollection.Single().Text);
        Assert.Equal((SubtitleRatingEnum)2, readBack.SubtitleCollection.Single().RatingCurrent);
    }

    [Fact]
    public async Task WriteTmpFileAsync_ExcludesUnratedAndRatingsAtOrAboveSelectedLevel()
    {
        // Current implementation retains only ratings with a lower enum value than the selected level.
        var model = CreateFileModel(
            CreateSubtitle(10, "A", (SubtitleRatingEnum)0),
            CreateSubtitle(20, "B", (SubtitleRatingEnum)1),
            CreateSubtitle(30, "C", (SubtitleRatingEnum)2),
            CreateSubtitle(40, "Unrated", null));
        var path = Path.Combine(_directory, "filtered.srt");

        var count = await _sut.WriteTmpFileAsync(model, path, (MyLanguageLevelEnum)2);
        var output = await _sut.ReadFileAsync(path);

        Assert.Equal(2, count);
        Assert.Equal(new[] { 1, 2 }, output.SubtitleCollection.Select(x => x.Position));
        Assert.Equal(new[] { "A", "B" }, output.SubtitleCollection.Select(x => x.Text));
        Assert.All(output.SubtitleCollection, x => Assert.Null(x.RatingCurrent));
    }

    [Fact]
    public async Task WriteFileAsync_ObservesCancellation()
    {
        var model = CreateFileModel(CreateSubtitle(1, "A", (SubtitleRatingEnum)0));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.WriteFileAsync(model, Path.Combine(_directory, "cancelled.srt"), cancellation.Token));
    }

    private string WriteInput(string name, string content)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
    }

    private static FileModel CreateFileModel(params SubtitleModel[] subtitles) => new()
    {
        SubtitleCollection = new System.Collections.ObjectModel.ObservableCollection<SubtitleModel>(subtitles)
    };

    private static SubtitleModel CreateSubtitle(int position, string text, SubtitleRatingEnum? rating) => new(Guid.NewGuid())
    {
        Position = position,
        Text = text,
        TimeFrom = TimeSpan.FromSeconds(position),
        TimeTo = TimeSpan.FromSeconds(position + 1),
        RatingOriginal = rating,
        RatingCurrent = rating
    };

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
