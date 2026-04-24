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
                FileType = FileType.Srt,
                Subtitles = new ObservableCollection<SubtitleModel>()
            };

            var fileInfo = new FileInfo(filePath);
            long totalBytes = fileInfo.Length;
            long bytesRead = 0;

            // Using UTF-8 with BOM as per project standards
            using var reader = new StreamReader(filePath, Encoding.UTF8, true);

            string line;
            var currentSub = new SubtitleModel();
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
                        FinalizeSubtitle(currentSub, textLines, rawBlock, fileModel);
                        currentSub = new SubtitleModel();
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
                            currentSub.Position = pos;
                            state = 1;
                        }
                        break;

                    case 1: // Expecting Timecode (00:00:00,000 --> 00:00:00,000)
                        var times = line.Split("-->");
                        if (times.Length == 2)
                        {
                            currentSub.TimeFrom = ParseSrtTime(times[0].Trim());
                            currentSub.TimeTo = ParseSrtTime(times[1].Trim());
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
                FinalizeSubtitle(currentSub, textLines, rawBlock, fileModel);
            }

            return fileModel;
        }

        private void FinalizeSubtitle(SubtitleModel sub, List<string> lines, StringBuilder raw, FileModel parent)
        {
            string combinedText = string.Join(Environment.NewLine, lines);

            // Extract and strip the {DIFF:X} tag
            var match = DiffRegex.Match(combinedText);
            if (match.Success)
            {
                string grade = match.Groups[1].Value.ToUpper();
                sub.RatingOriginal = sub.RatingCurrent = Enum.Parse<SubtitleRating>(grade);
                // Strip tag from display text
                sub.Text = DiffRegex.Replace(combinedText, "").Trim();
            }
            else
            {
                sub.Text = combinedText;
            }

            sub.OriginalBlock = raw.ToString();
            parent.Subtitles.Add(sub);
        }

        private TimeSpan ParseSrtTime(string srtTime)
        {
            // SRT format: 00:00:00,000
            // TimeSpan.Parse uses '.' for fractional seconds, SRT uses ','
            return TimeSpan.Parse(srtTime.Replace(',', '.'));
        }

        public Task WriteFileAsync(FileModel fileModel, string filePath, CancellationToken ct = default)
        {
            throw new NotImplementedException("Next step: Implement export with forced CRLF.");
        }
    }
}