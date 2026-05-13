// Subs.Ext\Tools\Rating.WPF\Sevices\FileService.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


using Rating.WPF.Enums;
using Rating.WPF.Models;
using Rating.WPF.General;

namespace Rating.WPF.Services
{
    public class FileService : IFileService
    {
        // Regex to find {DIFF:A} through {DIFF:E}
        private static readonly Regex DiffRegex = new Regex(@"\{DIFF:([A-E])\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async Task<FileModel> ReadFileAsync(string filePath, CancellationToken ct = default, IProgress<double> progress = null)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var fileModel = new FileModel
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileType = FileTypeEnum.Srt,
                SubtitleCollection = new ObservableCollection<SubtitleModel>()
            };

            var fileInfo = new FileInfo(filePath);
            long totalBytes = fileInfo.Length;
            long bytesRead = 0;

            // Using UTF-8 with BOM as per project standards
            using var reader = new StreamReader(filePath, Encoding.UTF8, true);

            string line;
            var currentSubtitle = new SubtitleModel(fileModel.PK);
            var textLines = new List<string>();
            var rawBlock = new StringBuilder();

            // State machine states
            int state = 0; // 0: Index, 1: Timecode, 2: Text

            while ((line = await reader.ReadLineAsync()) != null)
            {
                ct.ThrowIfCancellationRequested();

                // Track progress
                bytesRead += Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
                progress?.Report((double)bytesRead / totalBytes * 100);

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (state == 2) // End of a block
                    {
                        FinalizeSubtitle(currentSubtitle, textLines, rawBlock, fileModel);
                        currentSubtitle = new SubtitleModel(fileModel.PK);
                        textLines.Clear();
                        rawBlock.Clear();
                        state = 0;
                    }
                    continue;
                }

                rawBlock.AppendLine(line);

                switch (state)
                {
                    case 0: // Expecting Index
                        if (int.TryParse(line.Trim(), out int pos))
                        {
                            currentSubtitle.Position = pos;
                            state = 1;
                        }
                        break;

                    case 1: // Expecting Timecode (00:00:00,000 --> 00:00:00,000)
                        var times = line.Split("-->");
                        if (times.Length == 2)
                        {
                            currentSubtitle.TimeFrom = ParseSrtTime(times[0].Trim());
                            currentSubtitle.TimeTo = ParseSrtTime(times[1].Trim());
                            state = 2;
                        }
                        break;

                    case 2: // Expecting Text Lines
                        textLines.Add(line);
                        break;
                }
            }

            // Handle last sub if file doesn't end with a blank line
            if (state == 2 && textLines.Count > 0)
            {
                FinalizeSubtitle(currentSubtitle, textLines, rawBlock, fileModel);
            }

            return fileModel;
        }

        private void FinalizeSubtitle(SubtitleModel sub, List<string> lines, StringBuilder raw, FileModel parent)
        {
            // Join lines for the WPF UI
            string combinedText = string.Join(Environment.NewLine, lines);

            var match = DiffRegex.Match(combinedText);
            if (match.Success)
            {
                string grade = match.Groups[1].Value.ToUpper();
                // Use TryParse to prevent crashes on malformed tags
                if (Enum.TryParse<SubtitleRatingEnum>(grade, out var rating))
                {
                    sub.RatingOriginal = sub.RatingCurrent = rating;
                }
                // Clean the text for display
                sub.Text = DiffRegex.Replace(combinedText, "").Trim();
            }
            else
            {
                sub.Text = combinedText;
                sub.RatingOriginal = sub.RatingCurrent = null; // Default
            }

            sub.OriginalBlock = raw.ToString();
            parent.SubtitleCollection.Add(sub);
        }

        private TimeSpan ParseSrtTime(string srtTime)
        {
            // Ensure we handle the comma and use Exact parsing to prevent format exceptions
            string cleanedTime = srtTime.Replace(',', '.');
            if (TimeSpan.TryParseExact(cleanedTime, @"hh\:mm\:ss\.fff", null, out var ts))
            {
                return ts;
            }
            return TimeSpan.Zero; // Or handle error
        }

        public async Task<string> WriteFileAsync(FileModel fileModel, string filePath, CancellationToken ct = default)
        {
            // If no filePath provided → create a temp SRT file
            if (string.IsNullOrWhiteSpace(filePath))
            {
                string tempFolder = Path.GetTempPath();
                string tempFile = Path.Combine(tempFolder, $"{Guid.NewGuid()}.{Constants.TempFilesSrtEnding}");
                filePath = tempFile;
            }

            // Define the SRT-compliant newline (CRLF)
            const string srtNewLine = "\r\n";

            // Use UTF-8 with BOM for maximum compatibility with media players
            using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));

            foreach (var sub in fileModel.SubtitleCollection)
            {
                ct.ThrowIfCancellationRequested();

                // 1. Index
                await writer.WriteAsync($"{sub.Position}{srtNewLine}");

                // 2. Timecodes (Force 00:00:00,000 format)
                string timeFrom = sub.TimeFrom.ToString(@"hh\:mm\:ss\,fff");
                string timeTo = sub.TimeTo.ToString(@"hh\:mm\:ss\,fff");
                await writer.WriteAsync($"{timeFrom} --> {timeTo}{srtNewLine}");

                // 3. Text + Optional Tag
                string cleanText = sub.Text.Replace(Environment.NewLine, srtNewLine);

                // Only add the tag if the line has been rated (A-E)
                string tag = sub.RatingCurrent.HasValue ? $" {{DIFF:{sub.RatingCurrent}}}" : "";
                await writer.WriteAsync($"{cleanText}{tag}{srtNewLine}");

                // 4. Closing empty line
                await writer.WriteAsync(srtNewLine);
            }

            await writer.FlushAsync();

            return filePath;
        }

        public async Task<int> WriteTmpFileAsync(
            FileModel fileModel,
            string filePath,
            MyLanguageLevelEnum languageLevel,
            CancellationToken ct = default)
        {
            // If no filePath provided → create a temp SRT file
            if (string.IsNullOrWhiteSpace(filePath))
            {
                string tempFolder = Path.GetTempPath();
                filePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}.{Constants.TempFilesSrtEnding}");
            }

            // Define the SRT-compliant newline (CRLF)
            const string srtNewLine = "\r\n";

            int count = 0;

            using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));

            foreach (var sub in fileModel.SubtitleCollection)
            {
                ct.ThrowIfCancellationRequested();

                // Skip subs with no rating
                if (!sub.RatingCurrent.HasValue)
                    continue;

                // Keep only subs with difficulty HIGHER than user's level
                if ((int)sub.RatingCurrent.Value >= (int)languageLevel)
                    continue;

                count++;

                // 1. Index (renumber sequentially)
                await writer.WriteAsync($"{count}{srtNewLine}");

                // 2. Timecodes
                string timeFrom = sub.TimeFrom.ToString(@"hh\:mm\:ss\,fff");
                string timeTo = sub.TimeTo.ToString(@"hh\:mm\:ss\,fff");
                await writer.WriteAsync($"{timeFrom} --> {timeTo}{srtNewLine}");

                // 3. Text (no DIFF tag in filtered output)
                string cleanText = sub.Text.Replace(Environment.NewLine, srtNewLine);
                await writer.WriteAsync($"{cleanText}{srtNewLine}");

                // 4. Blank line
                await writer.WriteAsync(srtNewLine);
            }

            await writer.FlushAsync();

            return count;
        }
    }
}