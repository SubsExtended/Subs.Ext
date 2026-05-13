// Subs.Ext\Tools\Rating.WPF\Sevices\IFileService.cs

using Rating.WPF.Enums;
using Rating.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Rating.WPF.Services
{
    public interface IFileService
    {
        // Changed directoryPath to filePath to match logic
        Task<FileModel> ReadFileAsync(string filePath, CancellationToken ct = default, IProgress<double> progress = null);
        Task<string> WriteFileAsync(FileModel fileModel, string filePath, CancellationToken ct = default);
        Task<int> WriteTmpFileAsync(FileModel fileModel, string filePath, MyLanguageLevelEnum languageLevel, CancellationToken ct = default);
    }
}