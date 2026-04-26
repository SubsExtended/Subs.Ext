// Subs.Ext\Tools\Rating.WPF\Sevices\IFileService.cs

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Rating.WPF.Models;

namespace Rating.WPF.Services
{
    public interface IFileService
    {
        // Changed directoryPath to filePath to match logic
        Task<FileModel> ReadFileAsync(string filePath, CancellationToken ct = default, IProgress<double> progress = null);
        Task WriteFileAsync(FileModel fileModel, string filePath, CancellationToken ct = default);
    }
}